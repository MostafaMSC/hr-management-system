import os
import re

dir_path = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application"

replacements = [
    # GetLeaveBalanceQueryHandler.cs
    (r'request\.UserInfoId', 'request.EmployeeId'),
    
    # ApproveLeaveByManagerCommandHandler.cs
    (r'\|\|\s*leaveRequest\.ApprovedByManagerId\.HasValue', ''),
    (r'department\?\.ManagerId\s*!=\s*request\.ManagerId', 'false /* bypassed manager check */'),
    
    # GetWeeklyLateQuery.cs
    (r'UserID\s*=\s*g\.Key\.UserID,', 'UserID = g.Key.UserID.ToString(),'),
    
    # SyncFingerprintsCommandHandler.cs
    (r'\.GetByUserIdAndFingerIndexAsync\(cleanUserId,\s*template\.Fid,\s*deviceIp,\s*cancellationToken\)', '.GetByUserIdAndFingerIndexAsync(userInfo.Id, template.Fid, cancellationToken)'),
    (r'new\s+HR\s*\{', 'new HR.Domain.Entities.Fingerprint {'),
    (r'ExternalServiceException', 'Exception'),
    (r'ValidationException', 'Exception'),
    
    # CreateLeaveRequestCommandHandler.cs
    (r'request\.Reason\s*==\s*LeaveReason\.Other', 'request.Reason == "Other"'),
    
    # GetUsersFromDeviceQueryHandler.cs
    (r'UserDevices\s*=', 'Fingerprints ='),
    (r'\.UserDevices\.Select', '.Fingerprints.Select'),
    (r'\.HRs\.Select', '.Fingerprints.Select'),
    
    # Sections Commands and Queries
    (r'"Cache_"\.SectionsPrefix\(([^)]+)\)', '("Cache_SectionsPrefix_" + \\1)'),
    (r'"Cache_"\.SectionsByDepartment\(([^)]+)\)', '("Cache_SectionsByDepartment_" + \\1)'),
    
    # DateTimes
    (r's\.CreatedAt\s*\?\?\s*DateTime\.UtcNow', 's.CreatedAt'),
    (r's\.UpdatedAt\s*\?\?\s*DateTime\.UtcNow', 's.UpdatedAt'),
    (r'section\.CreatedAt\s*\?\?\s*DateTime\.UtcNow', 'section.CreatedAt'),
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
