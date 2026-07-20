import sys
import io
import json
import codecs
from zk import ZK

# Ensure UTF-8 output
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

if len(sys.argv) < 2:
    print(json.dumps({"success": False, "error": "Missing device IP argument"}, ensure_ascii=False))
    sys.exit(1)

device_ip = sys.argv[1]
device_port = 4370

try:
    zk = ZK(device_ip, port=device_port, timeout=10, verbose=False)
    conn = zk.connect()
    
    # Get all users to create UID -> UserID mapping
    users = conn.get_users()
    uid_to_badge_map = {}
    
    for user in users:
        # Map UID to Badge Number if it exists
        if user.user_id and user.user_id.strip():
            uid_to_badge_map[user.uid] = user.user_id.strip()
    
    # Get all fingerprints
    templates = conn.get_templates()
    
    templates_list = []
    for finger in templates:
        template_hex = codecs.encode(finger.template, 'hex').decode('ascii')
        
        # Determine the best UserID to match with database
        # Try mapped badge number first, then finger.user_id, finally finger.uid
        badge = uid_to_badge_map.get(finger.uid, "")
        if not badge and hasattr(finger, 'user_id'):
            badge = str(finger.user_id).strip()
            
        user_id_for_db = badge if badge else str(finger.uid)
        
        templates_list.append({
            "uid": finger.uid,
            "userId": user_id_for_db,
            "badgeNumber": badge,
            "fid": finger.fid,
            "valid": finger.valid,
            "template": template_hex,
            "size": finger.size
        })
    
    print(json.dumps({
        "success": True,
        "count": len(templates_list),
        "templates": templates_list
    }, ensure_ascii=False))
    
    conn.disconnect()

except Exception as e:
    import traceback
    print(json.dumps({"success": False, "error": str(e)}, ensure_ascii=False))
    sys.exit(1)