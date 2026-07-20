import sys
import io
import json
import base64
from zk import ZK, const
from zk.finger import Finger

# Ensure UTF-8 output
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

# Usage: python sync_full_user.py <device_ip> <temp_file_path>
if len(sys.argv) < 3:
    print(json.dumps({"success": False, "error": "Missing arguments"}, ensure_ascii=False))
    sys.exit(1)

device_ip = sys.argv[1]
temp_file = sys.argv[2]

try:
    with open(temp_file, 'r', encoding='utf-8') as f:
        user_data = json.load(f)

    user_id = user_data['user_id']
    name = user_data['name']
    password = user_data.get('password', '')
    
    # Robust card parsing
    card_str = str(user_data.get('card', '0')).strip()
    card = 0
    try:
        if card_str:
            card = int(card_str)
    except ValueError:
        pass

    privilege = int(user_data.get('privilege', 0))
    fingerprints = user_data.get('fingerprints', [])

    device_port = 4370
    # Higher timeout for combined operation
    zk = ZK(device_ip, port=device_port, timeout=20) 
    conn = None
    
    try:
        print(f"Connecting to {device_ip}...", file=sys.stderr)
        conn = zk.connect()
        conn.disable_device()

        # --- Step 1: Add/Update User Info ---
        print(f"Syncing user info for {user_id}...", file=sys.stderr)
        users = conn.get_users()
        existing_user = next((u for u in users if u.user_id == str(user_id)), None)
        
        if existing_user:
            uid = existing_user.uid
        else:
            uid = (max([u.uid for u in users], default=0) + 1)

        conn.set_user(
            uid=uid,
            name=name,
            privilege=privilege,
            password=password,
            card=card,
            user_id=str(user_id)
        )
        
        # We must refresh or get the user object again to ensure the library has the latest state 
        # for fingerprint association, although we already have the UID.
        
        # --- Step 2: Sync Fingerprints ---
        finger_count = 0
        if fingerprints:
            print(f"Syncing {len(fingerprints)} fingerprints...", file=sys.stderr)
            
            # Re-fetch user object to be safe for foreign key relation in library
            # (Optimization: We could construct the User object manually if we trust the UID)
            target_user = existing_user if existing_user else next((u for u in conn.get_users() if u.uid == uid), None)
            
            if target_user:
                finger_objects = []
                for fp in fingerprints:
                    try:
                        finger_index = int(fp['finger_index'])
                        template_base64 = fp['template']
                        template_data = base64.b64decode(template_base64)
                        
                        f_obj = Finger(target_user.uid, finger_index, 1, template_data)
                        finger_objects.append(f_obj)
                        finger_count += 1
                    except Exception as fe:
                        print(f"Warning: Failed to process finger {fp.get('finger_index')}: {fe}", file=sys.stderr)

                if finger_objects:
                    conn.save_user_template(target_user, finger_objects)
        
        # --- Step 3: Refresh and Finish ---
        try:
            conn.refresh_data()
        except:
            pass

        print(json.dumps({
            "success": True,
            "message": f"User {user_id} and {finger_count} fingerprints synced successfully",
            "data": {
                "uid": uid,
                "user_id": user_id,
                "finger_count": finger_count
            }
        }, ensure_ascii=False))

    except Exception as e:
        # Categorize common connection errors
        err_msg = str(e)
        if "timed out" in err_msg.lower() or "unreachable" in err_msg.lower():
             print(json.dumps({
                "success": False,
                "error": f"Connection failed: Device {device_ip} is unreachable or offline."
            }, ensure_ascii=False))
        else:
            print(json.dumps({
                "success": False,
                "error": f"Device operation failed: {err_msg}"
            }, ensure_ascii=False))
            
        print(f"Traceback: {err_msg}", file=sys.stderr)

    finally:
        if conn:
            try:
                conn.enable_device()
                conn.disconnect()
            except:
                pass

except Exception as wrapper_ex:
    print(json.dumps({
        "success": False,
        "error": f"Internal Script Error: {str(wrapper_ex)}"
    }, ensure_ascii=False))
