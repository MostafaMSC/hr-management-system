import sys
import io
import json
import codecs
from zk import ZK, const

# Ensure UTF-8 output
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

# Usage: python get_users.py <device_ip>
if len(sys.argv) < 2:
    print(json.dumps({"success": False, "error": "Missing arguments"}, ensure_ascii=False))
    sys.exit(1)

device_ip = sys.argv[1]
device_port = 4370

try:
    zk = ZK(device_ip, port=device_port, timeout=20) 
    conn = zk.connect()

    users = conn.get_users()
    users = conn.get_users()
    
    templates_by_uid = {}
    try:
        templates = conn.get_templates() 
        # Map templates by uid (internal int ID)
        for t in templates:
            uid = t.uid 
            if uid not in templates_by_uid:
                templates_by_uid[uid] = []
            
            fid = t.fid if hasattr(t, 'fid') else 0
            valid = t.valid if hasattr(t, 'valid') else 1
            
            # Proper encoding of template bytes to Hex string
            template_str = ""
            if hasattr(t, 'template') and t.template:
                # t.template is bytes, encode to hex string
                template_str = codecs.encode(t.template, 'hex').decode('ascii')

            templates_by_uid[uid].append({
                "FingerID": fid,
                "Template": template_str,
                "Size": len(t.template) if t.template else 0,
                "Valid": bool(valid)
            })
    except Exception as e:
        # If get_templates fails, just log it to debug_info (or stderr) and continue with users
        # We don't want to fail the whole user sync just because templates failed
        sys.stderr.write(f"Warning: Failed to get templates: {str(e)}\n")

    users_list = []
    for u in users:
        # u.uid is the internal int ID
        user_templates = templates_by_uid.get(u.uid, [])
        
        # Get privilege/role - try both fields
        privilege = None
        if hasattr(u, 'privilege'):
            privilege = u.privilege
        elif hasattr(u, 'role'):
            privilege = u.role
        
        # Map privilege to role name
        role_name = None
        if privilege is not None:
            if privilege == 0:
                role_name = "User"
            elif privilege == 14:
                role_name = "Administrator"
            else:
                role_name = f"Role_{privilege}"
        
        users_list.append({
            "UID": u.uid,
            "UserID": u.user_id, 
            "Name": u.name,
            "Card": u.card if hasattr(u, 'card') else None,
            "Privilege": privilege,  # Numeric privilege
            "Role": role_name,       # Human-readable role
            "Password": u.password if hasattr(u, 'password') else None,
            "Fingerprints": user_templates
        })

    print(json.dumps({
        "success": True,
        "count": len(users_list),
        "users": users_list
    }, ensure_ascii=False))

    conn.disconnect()

except Exception as e:
    import traceback
    print(json.dumps({
        "success": False,
        "error": str(e),
        "trace": traceback.format_exc()
    }, ensure_ascii=False))