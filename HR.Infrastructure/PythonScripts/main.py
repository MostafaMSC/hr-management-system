from fastapi import FastAPI, HTTPException
from fastapi.responses import JSONResponse
from pydantic import BaseModel
import sys
import io
import json
from typing import List, Optional
from datetime import datetime, timedelta
from zk import ZK, const

app = FastAPI(title="ZKTeco Attendance API")

# --- Models ---
class FingerprintDto(BaseModel):
    finger_index: int
    template: str # Base64 string

class SyncUserDto(BaseModel):
    user_id: str
    name: str
    password: Optional[str] = ""
    card: Optional[str] = "0"
    privilege: Optional[int] = 0
    fingerprints: List[FingerprintDto] = []

class AddEditUserDto(BaseModel):
    user_id: str
    name: str
    password: Optional[str] = ""
    card: Optional[str] = "0"
    privilege: Optional[int] = 0

class EnrollRequestDto(BaseModel):
    user_id: str
    temp_id: int

# --- Endpoints ---

@app.get("/api/devices/{device_ip}/logs")
async def get_device_logs(device_ip: str):
    try:
        zk = ZK(device_ip, port=4370, timeout=5)
        conn = zk.connect()

        users = conn.get_users()
        users_dict = {
            u.user_id: {"Name": u.name, "Role": getattr(u, "role", None)}
            for u in users
        }

        logs = conn.get_attendance()
        logs.reverse()

        thirty_days_ago = datetime.now() - timedelta(days=30)
        logs_list = []
        
        for log in logs:
            if log.timestamp < thirty_days_ago:
                continue

            punch_type = getattr(log, "punch", None)
            user = users_dict.get(log.user_id, {})

            logs_list.append({
                "userID": log.user_id,
                "name": user.get("Name", "Unknown"),
                "time": log.timestamp.isoformat(),
                "role": str(user.get("Role", "")),
                "deviceIP": device_ip,
                "checkStatus": str(punch_type) if punch_type is not None else "0",
                "logsType": 0
            })

        conn.disconnect()
        return logs_list
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/api/devices/{device_ip}/users")
async def get_users(device_ip: str):
    try:
        zk = ZK(device_ip, port=4370, timeout=5)
        conn = zk.connect()
        users = conn.get_users()
        templates = conn.get_templates()
        
        # Group templates by user
        user_templates = {}
        for t in templates:
            uid = str(t.uid)
            if uid not in user_templates:
                user_templates[uid] = []
            
            # Note: template handling might vary by ZK library version. 
            # In zkpy, t.template is usually bytes.
            import base64
            template_b64 = base64.b64encode(t.template).decode('utf-8')
            
            user_templates[uid].append({
                "finger_index": t.fid,
                "template": template_b64
            })
            
        result = []
        for u in users:
            uid = str(u.uid)
            result.append({
                "user_id": u.user_id,
                "name": u.name,
                "privilege": u.privilege,
                "password": u.password,
                "card": u.card,
                "fingerprints": user_templates.get(uid, [])
            })

        conn.disconnect()
        return result
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/api/devices/{device_ip}/users")
async def add_edit_user(device_ip: str, data: AddEditUserDto):
    try:
        zk = ZK(device_ip, port=4370, timeout=5)
        conn = zk.connect()
        # conn.set_user expects uid (internal id), name, privilege, password, group_id, user_id (string), card
        # we can just pass user_id for both uid and user_id if it's numeric, or generate one.
        # usually user_id is the string enroll number
        conn.set_user(
            uid=int(data.user_id), 
            name=data.name, 
            privilege=data.privilege, 
            password=data.password, 
            user_id=data.user_id,
            card=int(data.card) if data.card.isdigit() else 0
        )
        conn.disconnect()
        return {"success": True, "message": f"User {data.user_id} set successfully."}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.delete("/api/devices/{device_ip}/users/{user_id}")
async def delete_user(device_ip: str, user_id: str):
    try:
        zk = ZK(device_ip, port=4370, timeout=5)
        conn = zk.connect()
        conn.delete_user(uid=int(user_id))
        conn.disconnect()
        return {"success": True, "message": f"User {user_id} deleted."}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/api/devices/{device_ip}/enroll")
async def enroll_user(device_ip: str, data: EnrollRequestDto):
    try:
        # Note: Increase timeout here because enrollment is interactive and blocks
        zk = ZK(device_ip, port=4370, timeout=120)
        conn = zk.connect()
        # Start enrollment. The user has to place finger 3 times.
        # This function blocks until successful, failed, or timed out.
        conn.enroll_user(uid=int(data.user_id), temp_id=data.temp_id, user_id=data.user_id)
        conn.disconnect()
        return {"success": True, "message": f"User {data.user_id} enrolled successfully."}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/api/devices/{device_ip}/sync-time")
async def sync_time(device_ip: str):
    try:
        zk = ZK(device_ip, port=4370, timeout=5)
        conn = zk.connect()
        conn.set_time(datetime.now())
        conn.disconnect()
        return {"success": True, "message": "Time synced successfully."}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/api/devices/{device_ip}/sync-full-user")
async def sync_full_user(device_ip: str, data: SyncUserDto):
    try:
        zk = ZK(device_ip, port=4370, timeout=10)
        conn = zk.connect()
        uid = int(data.user_id)
        
        # 1. Set user info
        conn.set_user(
            uid=uid, 
            name=data.name, 
            privilege=data.privilege, 
            password=data.password, 
            user_id=data.user_id,
            card=int(data.card) if data.card.isdigit() else 0
        )
        
        # 2. Sync templates
        import base64
        for fp in data.fingerprints:
            template_bytes = base64.b64decode(fp.template)
            # Check length to determine if it's ZK9 or ZK10 (the library handles it but we pass bytes)
            # Depending on python-zk library, conn.save_user_template might be used
            # Some libraries use: set_user_template(uid, temp_id, template)
            try:
                conn.save_user_template(user=uid, template=template_bytes) # Check python-zk docs for exact method signature
            except AttributeError:
                # If save_user_template is not found, try set_user_template
                # zkpy's format: def set_user_template(self, uid, temp_id, template)
                conn.set_user_template(uid=uid, temp_id=fp.finger_index, template=template_bytes)
                
        conn.disconnect()
        return {"success": True, "message": f"User {data.user_id} fully synced."}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
