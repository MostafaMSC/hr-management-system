import os
import re

dir_path = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application\\Attendance\\ZKPython"
app_path = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application"

replacements = [
    # General Properties
    (r'\.EmployeeId\b', '.UserInfoId'),
    (r'\.Employee\b', '.UserInfo'),
    (r'\.StartTime\b', '.StartDate'),
    (r'\.EndTime\b', '.EndDate'),
    (r'\.LeaveReason\b', '.Reason'),
    
    # Missing properties on LeaveRequest to remove
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.AdditionalComment\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.BackupEmployeeId\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.BackupEmployee\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.BackupStatus\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.BackupComment\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.BackupRespondedAt\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.ManagerComment\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.HRComment\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.ApprovedByManager\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.ApprovedByHR\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.ApprovedByManagerAt\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.ApprovedByHRAt\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.RejectedBy\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.RejectedAt\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.ApprovedByManagerId\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.RejectedById\s*=\s*.*?;[ \t]*\r?\n?', ''),
    (r'(?m)^\s*(?:request|leave|[A-Za-z_]+)\.LeaveDate\s*=\s*.*?;[ \t]*\r?\n?', ''),

    # LeaveRequest Queries removals
    (r'(?m)^\s*AdditionalComment\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*BackupEmployeeId\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*BackupEmployee\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*BackupStatus\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*BackupComment\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*BackupRespondedAt\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*ManagerComment\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*HRComment\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*ApprovedByManager\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*ApprovedByHR\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*ApprovedByManagerAt\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*ApprovedByHRAt\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*RejectedBy\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*RejectedAt\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*ApprovedByManagerId\s*=\s*.*?,[ \t]*\r?\n?', ''),
    (r'(?m)^\s*RejectedById\s*=\s*.*?,[ \t]*\r?\n?', ''),

    # Fingerprint Repository
    (r'\.GetHRsByUserIdAsync', '.GetByUserIdAsync'),
    (r'\.GetAllHRsAsync', '.GetAllAsync'),
    (r'await _fingerprintRepository\.GetCountAsync\(\)', '(await _fingerprintRepository.GetAllAsync()).Count()'),

    # UserInfo Repository and Device properties
    (r'\.HRs', '.UserDevices'),
    (r'CacheKeys\.', '"Cache_".'),
    (r'Models\.Department', 'Entities.Department'),
    
    (r'\.GetByBiometricIdGlobalAsync\(', '.GetByIdAsync('),

    # Enum to string conversions for LeaveType
    (r'LeaveType\s*=\s*request\.LeaveType,', 'LeaveType = request.LeaveType.ToString(),'),
    (r'LeaveType\s*==\s*request\.LeaveType', 'LeaveType == request.LeaveType.ToString()'),

    # DateTime issues (e.g. DateTime? missing Date)
    (r'(?<=[A-Za-z0-9_])\.Date\b', '.GetValueOrDefault().Date'),
    
    # Gender ?? "" error etc. (operator ?? cannot be applied to UserType? and string)
    (r'\?\?\s*""', '?.ToString() ?? ""'),
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
                
                if content != orig_content:
                    with open(path, 'w', encoding='utf-8') as f:
                        f.write(content)
                    print(f"Updated {path}")

process_dir(dir_path)
process_dir(os.path.join(app_path, "Attendance", "Commands"))
process_dir(os.path.join(app_path, "Attendance", "Queries"))

print("Replacements done.")
