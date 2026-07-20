import os
import re

dir_path = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application\\Attendance\\ZKPython"

replacements = [
    # WorkHoursQueryHandler issue
    (r'l\.UserID\s*==\s*user\.BiometricId\s*\|\|\s*l\.UserID\s*==\s*user\.Username', 'l.UserInfoId == user.Id'),
    (r'l\.UserID\s*==\s*user\.BiometricId', 'l.UserInfoId == user.Id'),

    # LeaveRequestResponseDto arguments mapping
    (r'lr\.AdditionalComment', 'null /* AdditionalComment */'),
    (r'lr\.BackupEmployeeId', 'null /* BackupEmployeeId */'),
    (r'lr\.BackupEmployee\?\.Username', 'null /* BackupEmployee */'),
    (r'lr\.BackupStatus', 'null /* BackupStatus */'),
    (r'lr\.BackupComment', 'null /* BackupComment */'),
    (r'lr\.BackupRespondedAt', 'null /* BackupRespondedAt */'),
    (r'lr\.ManagerComment', 'null /* ManagerComment */'),
    (r'lr\.HRComment', 'null /* HRComment */'),
    (r'lr\.ApprovedByManager\?\.Username', 'null /* ApprovedByManager */'),
    (r'lr\.ApprovedByHR\?\.Username', 'null /* ApprovedByHR */'),
    (r'lr\.ApprovedByManagerAt', 'null /* ApprovedByManagerAt */'),
    (r'lr\.ApprovedByHRAt', 'null /* ApprovedByHRAt */'),
    (r'lr\.RejectedBy\?\.Username', 'null /* RejectedBy */'),
    (r'lr\.RejectedAt', 'null /* RejectedAt */'),
    
    (r'lr\.LeaveType,', '(HR.Domain.Enums.LeaveType)Enum.Parse(typeof(HR.Domain.Enums.LeaveType), string.IsNullOrEmpty(lr.LeaveType) ? "Annual" : lr.LeaveType),'),
    (r'lr\.Reason,', '(HR.Domain.Enums.LeaveReason)Enum.Parse(typeof(HR.Domain.Enums.LeaveReason), string.IsNullOrEmpty(lr.Reason) ? "Other" : lr.Reason),'),
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

print("Replacements done.")
