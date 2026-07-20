import sys
import io
import json
from datetime import datetime
from zk import ZK

# Ensure UTF-8 output
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

if len(sys.argv) < 2:
    print(json.dumps({"success": False, "error": "Device IP argument is required"}, ensure_ascii=False))
    sys.exit(1)

device_ip = sys.argv[1]
device_port = 4370

try:
    zk = ZK(device_ip, port=device_port, timeout=5)
    conn = zk.connect()
    
    # Get current server time
    now = datetime.now()
    
    # Set device time
    conn.set_time(now)
    
    print(json.dumps({
        "success": True, 
        "message": f"Device time set to: {now.strftime('%Y-%m-%d %H:%M:%S')}",
        "synced_time": str(now)
    }, ensure_ascii=False))
    
    conn.disconnect()

except Exception as e:
    print(json.dumps({
        "success": False, 
        "error": str(e)
    }, ensure_ascii=False))
