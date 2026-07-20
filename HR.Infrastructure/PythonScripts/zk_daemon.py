import socket
import json
import threading
import time
from zk import ZK

HOST = '127.0.0.1'
PORT = 9999

# Cache for device connections
# { ip: ZK_Connection }
connections = {}
connections_lock = threading.Lock()

# Per-IP locks to prevent checking/interacting with one device blocking others
device_locks = {}
device_locks_lock = threading.Lock()

def get_device_lock(ip):
    with device_locks_lock:
        if ip not in device_locks:
            device_locks[ip] = threading.Lock()
        return device_locks[ip]

def purge_connection(ip):
    with connections_lock:
        if ip in connections:
            conn = connections[ip]
            try:
                conn.disconnect()
            except:
                pass
            del connections[ip]
            print(f"❌ Cached connection for {ip} was purged.")

def get_connection(ip):
    # Use per-device lock so checking/connecting to device A doesn't block device B
    lock = get_device_lock(ip)
    with lock:
        with connections_lock:
            cached_conn = connections.get(ip)
        
        if cached_conn:
            try:
                # Basic check to see if connection is flagged active
                if cached_conn.is_connect:
                    return cached_conn
            except:
                pass
            # If not active or error, evict
            purge_connection(ip)

        try:
            print(f"🔌 Connecting to device: {ip}...")
            zk = ZK(ip, port=4370, timeout=5)
            conn = zk.connect()
            with connections_lock:
                connections[ip] = conn
            print(f"✅ Successfully established connection to {ip}")
            return conn
        except Exception as e:
            print(f"Failed to connect to {ip}: {e}")
            return None

def heartbeat_check():
    print("💓 Heartbeat background thread started.")
    while True:
        time.sleep(15)  # Check every 15 seconds
        with connections_lock:
            ips = list(connections.keys())
        
        for ip in ips:
            with connections_lock:
                conn = connections.get(ip)
            if not conn:
                continue
            
            # Try to acquire per-device lock. If it's busy with a command, skip heartbeat for now.
            lock = get_device_lock(ip)
            if lock.acquire(blocking=False):
                try:
                    # Lightweight query check
                    conn.get_time()
                except Exception as e:
                    print(f"⚠️ Heartbeat failed for {ip}: {e}. Evicting from cache.")
                    purge_connection(ip)
                finally:
                    lock.release()

def handle_client(client_socket):
    ip = None
    try:
        data = client_socket.recv(40960).decode('utf-8')
        if not data:
            return
        
        request = json.loads(data)
        command = request.get('command')
        ip = request.get('ip')
        
        if not ip:
            client_socket.send(json.dumps({"success": False, "error": "IP missing"}).encode('utf-8'))
            return

        conn = get_connection(ip)
        if not conn:
            client_socket.send(json.dumps({"success": False, "error": f"Could not connect to {ip}"}).encode('utf-8'))
            return

        lock = get_device_lock(ip)
        with lock:
            result = {"success": False}

            if command == 'get_logs':
                logs = conn.get_attendance()
                logs_data = []
                for l in reversed(logs):
                    logs_data.append({
                        "UserID": l.user_id,
                        "Time": str(l.timestamp),
                        "Punch": getattr(l, "punch", None)
                    })
                result = {"success": True, "data": logs_data}
            
            elif command == 'get_users':
                users = conn.get_users()
                users_data = []
                for u in users:
                    users_data.append({
                        "UserID": u.user_id,
                        "Name": u.name,
                        "Card": getattr(u, "card", None)
                    })
                result = {"success": True, "data": users_data}
            
            elif command == 'get_device_time':
                device_time = conn.get_time()
                result = {"success": True, "device_time": str(device_time)}
            
            response = json.dumps(result).encode('utf-8')
            client_socket.send(response)
            
    except Exception as e:
        print(f"Error handling client request for device {ip}: {e}")
        if ip:
            purge_connection(ip)
        try:
            client_socket.send(json.dumps({"success": False, "error": str(e)}).encode('utf-8'))
        except:
            pass
    finally:
        client_socket.close()

def start_daemon():
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.bind((HOST, PORT))
    server.listen(5)
    print(f"🐍 ZK Daemon listening on {HOST}:{PORT}")
    
    # Start heartbeat checker in a daemon thread
    heartbeat_thread = threading.Thread(target=heartbeat_check, daemon=True)
    heartbeat_thread.start()
    
    while True:
        client, addr = server.accept()
        client_handler = threading.Thread(target=handle_client, args=(client,))
        client_handler.start()

if __name__ == "__main__":
    start_daemon()
