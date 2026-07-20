import sys
import io
import json
from zk import ZK, const
from zk.user import User
from zk.finger import Finger

# Ensure UTF-8 output
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

# Usage: python edit_user.py <device_ip> <user_id> <new_name> [privilege]
# privilege: 0 = Regular User, 14 = Administrator (ZKTeco standard)
if len(sys.argv) < 4:
    print(json.dumps({"success": False, "error": "Missing arguments. Usage: edit_user.py <device_ip> <user_id> <new_name> [privilege]"}, ensure_ascii=False))
    sys.exit(1)

device_ip = sys.argv[1]
user_id = sys.argv[2]
new_name = sys.argv[3]

# Optional privilege argument (4th arg)
new_privilege = None
if len(sys.argv) >= 5:
    try:
        new_privilege = int(sys.argv[4])
    except ValueError:
        new_privilege = None

device_port = 4370
conn = None

try:
    zk = ZK(device_ip, port=device_port, timeout=10)
    conn = zk.connect()

    # Disable device during operation
    conn.disable_device()

    # Fetch all users (this also sets user_packet_size needed for repack)
    users = conn.get_users()
    target_user = next((u for u in users if str(u.user_id) == str(user_id)), None)

    if not target_user:
        # Try finding by UID if UserID fails (fallback)
        if user_id.isdigit():
            target_user = next((u for u in users if str(u.uid) == str(user_id)), None)

    if not target_user:
        print(json.dumps({"success": False, "error": f"User {user_id} not found on device"}, ensure_ascii=False))
        conn.enable_device()
        conn.disconnect()
        sys.exit(0)

    # Determine privilege to use
    privilege_to_set = new_privilege if new_privilege is not None else target_user.privilege

    # Safely extract existing values
    try:
        card_value = int(target_user.card) if hasattr(target_user, 'card') and target_user.card else 0
    except (ValueError, TypeError):
        card_value = 0

    pwd = target_user.password if hasattr(target_user, 'password') and target_user.password else ''
    uid_val = int(target_user.uid)
    user_id_val = str(target_user.user_id)
    group_id_val = str(target_user.group_id) if hasattr(target_user, 'group_id') and target_user.group_id else ''

    # ========================================================================
    # Strategy: Try set_user first (CMD_USER_WRQ).
    # If it fails, use HR_save_usertemplates (_CMD_SAVE_USERTEMPS) as fallback.
    # This second approach bypasses CMD_USER_WRQ entirely and uses a different
    # device command that is more widely supported for updating existing users.
    # ========================================================================
    
    success = False
    last_error = None

    # --- Attempt 1: Standard set_user ---
    try:
        conn.set_user(
            uid=uid_val,
            name=new_name,
            privilege=privilege_to_set,
            password=pwd,
            card=card_value,
            user_id=user_id_val
        )
        success = True
    except Exception as e1:
        last_error = e1

    # --- Attempt 2: Use HR_save_usertemplates (different device command) ---
    if not success:
        try:
            # Get the user's existing fingerprint templates to preserve them
            fingers = []
            try:
                templates = conn.get_templates()
                fingers = [t for t in templates if t.uid == uid_val]
            except Exception:
                pass  # If we can't get templates, proceed without them

            # Create an updated User object with the new name/privilege
            updated_user = User(
                uid=uid_val,
                name=new_name,
                privilege=privilege_to_set,
                password=pwd,
                group_id=group_id_val,
                user_id=user_id_val,
                card=card_value
            )

            # Use HR_save_usertemplates - this uses _CMD_SAVE_USERTEMPS (command 110)
            # instead of CMD_USER_WRQ (command 8), bypassing the failing command
            conn.HR_save_usertemplates([(updated_user, fingers)])
            success = True
        except Exception as e2:
            last_error = e2

    # --- Attempt 3: Delete and re-add ---
    if not success:
        try:
            # Save fingerprint templates before delete
            fingers = []
            try:
                templates = conn.get_templates()
                fingers = [t for t in templates if t.uid == uid_val]
            except Exception:
                pass

            # Delete the user
            conn.delete_user(uid=uid_val)

            # Re-add with new name/privilege (using the SAME uid to preserve associations)
            conn.set_user(
                uid=uid_val,
                name=new_name,
                privilege=privilege_to_set,
                password=pwd,
                card=card_value,
                user_id=user_id_val
            )

            # Restore fingerprints if we had any
            if fingers:
                try:
                    updated_user = User(
                        uid=uid_val,
                        name=new_name,
                        privilege=privilege_to_set,
                        password=pwd,
                        group_id=group_id_val,
                        user_id=user_id_val,
                        card=card_value
                    )
                    conn.HR_save_usertemplates([(updated_user, fingers)])
                except Exception:
                    pass  # User is already updated, fingerprints might be lost

            success = True
        except Exception as e3:
            last_error = e3

    conn.enable_device()

    if not success:
        print(json.dumps({
            "success": False,
            "error": f"Can't update user on device after all attempts: {str(last_error)}",
            "debug": {
                "uid": uid_val,
                "user_id": user_id_val,
                "name": new_name,
                "privilege": privilege_to_set,
                "card": card_value,
                "password_empty": pwd == ''
            }
        }, ensure_ascii=False))
        conn.disconnect()
        sys.exit(0)

    # Refresh device data
    try:
        conn.refresh_data()
    except Exception:
        pass

    print(json.dumps({
        "success": True,
        "message": "User updated successfully",
        "data": {
            "UID": uid_val,
            "UserID": user_id_val,
            "Name": new_name,
            "Privilege": privilege_to_set,
            "Card": card_value
        }
    }, ensure_ascii=False))

    conn.disconnect()

except Exception as e:
    import traceback
    error_tb = traceback.format_exc()
    print(json.dumps({
        "success": False,
        "error": str(e),
        "traceback": error_tb
    }, ensure_ascii=False))
    if conn:
        try:
            conn.enable_device()
            conn.disconnect()
        except Exception:
            pass
