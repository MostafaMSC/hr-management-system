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
    
    # Get device time
    device_time = conn.get_time()

    # Get last log
    logs = conn.get_attendance()
    last_log_time = "No logs found"
    if logs:
        # Sort by timestamp desc
        logs.sort(key=lambda x: x.timestamp, reverse=True)
        last_log_time = str(logs[0].timestamp)
    
    print(json.dumps({
        "success": True, 
        "device_time": str(device_time),
        "server_time": str(datetime.now()),
        "last_log_time": last_log_time
    }, ensure_ascii=False))
    
    conn.disconnect()

except Exception as e:
    print(json.dumps({
        "success": False, 
        "error": str(e)
    }, ensure_ascii=False))
