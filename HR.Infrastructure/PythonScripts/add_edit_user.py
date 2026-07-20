import sys
import io
import json
from zk import ZK, const

# Ensure UTF-8 output
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

# Usage: python add_edit_user.py <device_ip> <temp_file_path>
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
    input_password = user_data.get('password', '')
    
    # Robust card parsing
    card_str = str(user_data.get('card', '0')).strip()
    card = 0
    try:
        if card_str:
            card = int(card_str)
    except ValueError:
        print(f"Warning: Could not parse card ID '{card_str}' as integer. Using 0.", file=sys.stderr)

    privilege = int(user_data.get('privilege', 0))

    device_port = 4370
    zk = ZK(device_ip, port=device_port, timeout=10) # 10s timeout
    conn = None
    
    try:
        print(f"Connecting to {device_ip}...", file=sys.stderr)
        conn = zk.connect()
        conn.disable_device()

        # Find existing user by user_id string
        print(f"Checking existing users for ID {user_id}...", file=sys.stderr)
        users = conn.get_users()
        existing_user = next((u for u in users if u.user_id == str(user_id)), None)
        
        if existing_user:
            uid = existing_user.uid
            print(f"Found existing user with UID {uid}", file=sys.stderr)
            # Preserve password if input is empty
            if not input_password and hasattr(existing_user, 'password') and existing_user.password:
                password = existing_user.password
            else:
                password = input_password
        else:
            uid = (max([u.uid for u in users], default=0) + 1)
            print(f"Creating new user with UID {uid}", file=sys.stderr)
            password = input_password

        # set_user works for both add and edit
        conn.set_user(
            uid=uid,
            name=name,
            privilege=privilege,
            password=password,
            card=card,
            user_id=str(user_id)
        )
        
        # Some devices need refresh_data to show the user in UI
        try:
            print("Refreshing device data...", file=sys.stderr)
            conn.refresh_data()
        except Exception as re:
            print(f"Warning: Could not refresh data: {str(re)}", file=sys.stderr)

        print(json.dumps({
            "success": True,
            "message": f"User {user_id} added/updated successfully on device",
            "data": {
                "uid": uid,
                "user_id": user_id,
                "name": name
            }
        }, ensure_ascii=False))

    finally:
        if conn:
            try:
                conn.enable_device()
                conn.disconnect()
            except:
                pass

except Exception as e:
    print(json.dumps({
        "success": False,
        "error": f"Internal Error: {str(e)}"
    }, ensure_ascii=False))
    # Still want to see stack trace in stderr if possible? No, keep it clean.
    print(f"Traceback: {str(e)}", file=sys.stderr)
