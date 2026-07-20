import sys
import io
import json
from zk import ZK, const

# Ensure UTF-8 output
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

# Usage: python delete_user.py <device_ip> <user_id>
if len(sys.argv) < 3:
    print(json.dumps({"success": False, "error": "Missing arguments"}, ensure_ascii=False))
    sys.exit(1)

device_ip = sys.argv[1]
user_id = sys.argv[2]
device_port = 4370

try:
    zk = ZK(device_ip, port=device_port, timeout=5)
    conn = zk.connect()

    # Disable device during operation
    conn.disable_device()

    success = False
    details = []
    
    # Helper to find user
    def find_user(user_id):
        print(f"Fetching users to find ID: {user_id}", file=sys.stderr)
        all_users = conn.get_users()
        target_str = str(user_id).strip()
        
        for u in all_users:
            u_id_str = str(u.user_id).strip()
            if u_id_str == target_str:
                return u
            # Also try matching against UID if user_id is integer-like
            if target_str.isdigit() and u.uid == int(target_str):
                return u
        return None

    target_user = find_user(user_id)
    
    if not target_user:
        success = True
        details.append(f"User {user_id} not found on device. Nothing to delete.")
        print(f"User {user_id} not found on device.", file=sys.stderr)
    else:
        print(f"Found user: UID={target_user.uid}, UserID={target_user.user_id}. Starting deletion...", file=sys.stderr)
        details.append(f"Found user: UID={target_user.uid}, UserID={target_user.user_id}")
        
        # STEP 1: Delete all fingerprints first
        fingerprints_deleted = 0
        try:
            print(f"🗑️ Deleting fingerprints for UID={target_user.uid}", file=sys.stderr)
            
            # Get all templates for this user
            templates = conn.get_templates()
            user_templates = [t for t in templates if t.uid == target_user.uid]
            
            print(f"Found {len(user_templates)} fingerprint templates", file=sys.stderr)
            
            # Delete each fingerprint template
            for template in user_templates:
                try:
                    conn.delete_user_template(uid=target_user.uid, temp_id=template.fid)
                    fingerprints_deleted += 1
                    print(f"Deleted fingerprint FID={template.fid}", file=sys.stderr)
                except Exception as fp_err:
                    print(f"Error deleting fingerprint FID={template.fid}: {fp_err}", file=sys.stderr)
            
            # Also try deleting all templates at once (temp_id=13)
            try:
                conn.delete_user_template(uid=target_user.uid, temp_id=13)
                details.append(f"Deleted {fingerprints_deleted} fingerprints using individual deletion + bulk cleanup")
            except Exception as bulk_err:
                details.append(f"Deleted {fingerprints_deleted} fingerprints (bulk cleanup failed: {bulk_err})")
                
        except Exception as fp_error:
            details.append(f"Fingerprint deletion error: {str(fp_error)}")
            print(f"⚠️ Fingerprint deletion error: {fp_error}", file=sys.stderr)
        
        # STEP 2: Delete the user with multiple strategies
        methods = ["Standard", "OverwriteDelete", "ForceClear"]
        for method in methods:
            try:
                if method == "Standard":
                    print(f"Applying method: {method}", file=sys.stderr)
                    conn.delete_user(uid=target_user.uid)
                    
                elif method == "OverwriteDelete":
                    print(f"Applying method: {method} (clear user data first)", file=sys.stderr)
                    # Overwrite user with blank data
                    conn.set_user(
                        uid=target_user.uid, 
                        name="", 
                        privilege=0, 
                        password="", 
                        user_id=str(user_id)
                    )
                    conn.delete_user(uid=target_user.uid)
                    
                elif method == "ForceClear":
                    print(f"Applying method: {method} (mark as deleted)", file=sys.stderr)
                    # Set user to "DELETED" state
                    conn.set_user(
                        uid=target_user.uid, 
                        name="DELETED", 
                        privilege=0,
                        password="",
                        user_id="0"
                    )
                    conn.delete_user(uid=target_user.uid)

                # Verification after each method
                conn.refresh_data()
                remaining_users = conn.get_users()
                still_exists = any(
                    str(u.user_id).strip() == str(user_id).strip() or u.uid == target_user.uid 
                    for u in remaining_users
                )
                
                if not still_exists:
                    print(f"✅ Success with method: {method}", file=sys.stderr)
                    success = True
                    details.append(f"User deleted successfully using {method}")
                    break
                else:
                    print(f"❌ Method {method} failed verification.", file=sys.stderr)
                    details.append(f"Method {method} failed (user still visible)")
                    
            except Exception as e:
                print(f"⚠️ Error using method {method}: {str(e)}", file=sys.stderr)
                details.append(f"Error in {method}: {str(e)}")

        # Final verification
        if success:
            # Double-check fingerprints are gone
            try:
                remaining_templates = conn.get_templates()
                remaining_user_fps = [t for t in remaining_templates if t.uid == target_user.uid]
                
                if remaining_user_fps:
                    details.append(f"⚠️ Warning: {len(remaining_user_fps)} fingerprints still remain in template report")
            except Exception as fp_check_err:
                details.append(f"Could not verify remaining fingerprints: {fp_check_err}")

    if success:
        print(json.dumps({
            "success": True,
            "message": "User and fingerprints deleted completely",
            "details": details,
            "data": {
                "UserID": user_id,
                "NotFound": (target_user is None),
                "FingerprintsDeleted": fingerprints_deleted
            }
        }, ensure_ascii=False))
    else:
        print(json.dumps({
            "success": False,
            "error": "Failed to completely delete user after multiple methods",
            "details": details
        }, ensure_ascii=False))

except Exception as e:
    print(json.dumps({
        "success": False,
        "error": str(e)
    }, ensure_ascii=False))

finally:
    if conn:
        try:
            conn.enable_device()
            conn.disconnect()
        except Exception:
            pass