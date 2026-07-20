import sys
import io
import json
from zk import ZK, const

# Ensure UTF-8 output
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

# Usage: python enroll_user.py <device_ip> <user_id> [temp_id]
if len(sys.argv) < 3:
    print(json.dumps({"success": False, "error": "Missing arguments"}, ensure_ascii=False))
    sys.exit(0)

device_ip = sys.argv[1]
user_id = sys.argv[2] # This is the user_id string (e.g., "1001")
temp_id = int(sys.argv[3]) if len(sys.argv) > 3 else 0

device_port = 4370
zk = ZK(device_ip, port=device_port, timeout=60) # Increased timeout for enrollment
conn = None

try:
    print(f"Connecting to {device_ip}...", file=sys.stderr)
    conn = zk.connect()
    
    # Check if user exists
    users = conn.get_users()
    user = next((u for u in users if u.user_id == str(user_id)), None)
    
    if not user:
        print(json.dumps({"success": False, "error": f"User {user_id} not found on device"}, ensure_ascii=False))
        sys.exit(0)

    print(f"Starting enrollment for User {user_id}, Finger {temp_id}. Please press finger 3 times...", file=sys.stderr)
    
    # enroll_user(self, uid=0, temp_id=0, user_id='')
    # The library uses either uid or user_id. We'll pass both to be safe/exact based on library logic
    result = conn.enroll_user(uid=user.uid, temp_id=temp_id, user_id=user_id)
    
    if result:
        print(json.dumps({
            "success": True, 
            "message": "Enrollment successful",
            "data": {
                "user_id": user_id,
                "finger_index": temp_id
            }
        }, ensure_ascii=False))
    else:
        print(json.dumps({"success": False, "error": "Enrollment failed or timed out"}, ensure_ascii=False))

except Exception as e:
    print(json.dumps({
        "success": False,
        "error": f"Internal Error: {str(e)}"
    }, ensure_ascii=False))
    print(f"Traceback: {str(e)}", file=sys.stderr)

finally:
    if conn:
        try:
            conn.disconnect()
        except:
            pass
