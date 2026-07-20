import sys
import io
import json
import base64
from zk import ZK, const
from zk.finger import Finger

# Ensure UTF-8 output
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

# Usage: python sync_fingerprints.py <device_ip> <temp_file_path>
if len(sys.argv) < 3:
    print(json.dumps({"success": False, "error": "Missing arguments"}, ensure_ascii=False))
    sys.exit(1)

device_ip = sys.argv[1]
temp_file = sys.argv[2]

try:
    with open(temp_file, 'r', encoding='utf-8') as f:
        payload = json.load(f)

    user_id = payload['user_id']
    fingerprints = payload['fingerprints']

    device_port = 4370
    zk = ZK(device_ip, port=device_port, timeout=15) # Longer timeout for fingerprints
    conn = None
    
    try:
        print(f"Connecting to {device_ip} for fingerprint sync...", file=sys.stderr)
        conn = zk.connect()
        conn.disable_device()

        # Find the user's UID on the device first
        print(f"Checking user {user_id} on device...", file=sys.stderr)
        users = conn.get_users()
        target_user = next((u for u in users if u.user_id == str(user_id)), None)

        if not target_user:
            print(json.dumps({
                "success": False,
                "error": f"User {user_id} not found on device. Add user profile first."
            }, ensure_ascii=False))
            sys.exit(0) # Exit gracefully so backend sees the error in JSON

        finger_objects = []
        count = 0
        for fp in fingerprints:
            finger_index = int(fp['finger_index'])
            template_base64 = fp['template']
            template_data = base64.b64decode(template_base64)
            
            print(f"Preparing finger {finger_index} (size: {len(template_data)} bytes)...", file=sys.stderr)
            
            # Create Finger object: uid, fid, valid, template
            f_obj = Finger(target_user.uid, finger_index, 1, template_data)
            finger_objects.append(f_obj)
            count += 1
        
        if finger_objects:
            print(f"Saving {len(finger_objects)} fingerprints to device...", file=sys.stderr)
            # The library's save_user_template expects (User object, list of Finger objects)
            conn.save_user_template(target_user, finger_objects)

        print(json.dumps({
            "success": True,
            "message": f"Successfully synced {count} fingerprints for user {user_id}",
            "data": {
                "user_id": user_id,
                "synced_count": count
            }
        }, ensure_ascii=False))

    finally:
        if conn:
            try:
                conn.enable_device()
                conn.disconnect()
            except Exception:
                pass

except Exception as e:
    print(json.dumps({
        "success": False,
        "error": f"Fingerprint Sync Error: {str(e)}"
    }, ensure_ascii=False))
    print(f"Traceback: {str(e)}", file=sys.stderr)
