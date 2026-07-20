import os
import re

dir_path = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application\\Attendance\\ZKPython"
app_path = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application"

replacements = [
    # More removals for LeaveRequest
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.ApprovedByHRId\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.ApprovedByManagerId\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.RejectedById\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*ApprovedByHRId\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*ApprovedByManagerId\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*RejectedById\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*AdditionalComment\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*BackupEmployeeId\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*BackupEmployee\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*BackupStatus\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*BackupComment\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*BackupRespondedAt\s*=\s*.*?,[ \t]*\r?\n?', ''),

    # ManagerId on Department
    (r'department\.ManagerId', '0 /* department.ManagerId */'),
    
    # Queries properties
    (r'public int EmployeeId \{ get; set; \}', 'public int UserInfoId { get; set; }'),
    (r'public int\? EmployeeId \{ get; set; \}', 'public int? UserInfoId { get; set; }'),

    # AllDevices cache key issue
    (r'"Cache_"\.AllDevices', '"Cache_AllDevices"'),
    (r'CacheKeys\.AllDevices', '"Cache_AllDevices"'),
]

def process_dir(d):
    for root, _, files in os.walk(d):
        for file in files:
            if file.endswith(".cs"):
                path = os.path.join(root, file)
                with open(path, 'r', encoding='utf-8-sig') as f:
                    content = f.read()
                
                orig_content = content
                for pattern, repl in replacements:
                    content = re.sub(pattern, repl, content)
                
                content = content.replace('!= UserType.HR', '!= "HR"')
                content = content.replace('!= UserType.Manager', '!= "Manager"')
                content = content.replace('!= UserType.Admin', '!= "Admin"')
                
                if content != orig_content:
                    with open(path, 'w', encoding='utf-8') as f:
                        f.write(content)
                    print(f"Updated {path}")

process_dir(dir_path)
process_dir(os.path.join(app_path, "Attendance", "Commands"))
process_dir(os.path.join(app_path, "Attendance", "Queries"))

print("Replacements done.")
