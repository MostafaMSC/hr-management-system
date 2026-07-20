import os
import re

dir_path = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application"

replacements = [
    # GetLeaveBalanceQueryHandler.cs
    (r'balance\.TotalDays', 'balance.AnnualDays'),
    (r'balance\.RemainingDays', 'balance.RemainingAnnualDays'),
    (r'balance\.TotalHours', '(balance.AnnualDays * 8)'),
    (r'balance\.RemainingHours', '(balance.RemainingAnnualDays * 8)'),
    (r'TotalDays:\s*\d+,', 'AnnualDays: 30,'),
    (r'RemainingDays:\s*\d+,', 'RemainingAnnualDays: 30,'),
    (r'TotalHours:\s*\d+,', ''),
    (r'RemainingHours:\s*\d+', ''),
    
    # GetEmployeeLeaveRequestsQueryHandler.cs missing CreatedAt
    (r'null\s*\/\*\s*RejectedAt\s*\*\/\s*\)\)', 'null /* RejectedAt */, lr.CreatedAt))'),
    (r'null\s*\/\*\s*RejectedAt\s*\*\/\s*,\s*lr\.CreatedAt\s*\)\)', 'null /* RejectedAt */, lr.CreatedAt))'), # Prevent duplicates
    
    # UpdateUserDevicesCommand.cs
    (r'user\.Fingerprints', '(await _fingerprintRepository.GetByUserIdAsync(user.Id, cancellationToken))'),
    (r'u\.Fingerprints', '(await _fingerprintRepository.GetByUserIdAsync(user.Id, cancellationToken))'),
    
    # SyncFingerprintsCommandHandler.cs
    (r'existing\.BiometricId\s*=\s*[^;]+;', ''),
    (r'existing\.Username\s*=\s*[^;]+;', ''),
    (r'BiometricId\s*=\s*[^,]+,', ''),
    (r'Username\s*=\s*[^,]+,', ''),
    (r'new\s+Exception\(([^,]+),\s*([^,]+),\s*([^)]+)\)', 'new Exception(\\1, \\3)'),
    
    # GetSectionsQuery.cs
    (r'"Cache_SectionsByDepartment"\(request\.DepartmentId\.Value\)', '($"Cache_SectionsByDepartment_{request.DepartmentId.Value}")'),
    (r'"Cache_SectionsPrefix"\(request\.DepartmentId\.Value\)', '($"Cache_SectionsPrefix_{request.DepartmentId.Value}")'),
    (r's\.CreatedAt\s*\?\?\s*DateTime\.UtcNow', 's.CreatedAt'),
    (r's\.UpdatedAt\s*\?\?\s*DateTime\.UtcNow', 's.UpdatedAt'),
    
    # Logs Command Handlers
    (r'UserID\s*=\s*log\.UserId,', 'UserInfoId = user.Id,'),
    (r'Name\s*=\s*log\.Name,', ''),
    (r'Card\s*=\s*log\.Card,', ''),
    (r'Role\s*=\s*log\.Role,', ''),
    (r'DeviceIP\s*=\s*log\.DeviceIP,', ''),
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
                
                # Deduplicate CreatedAt if added multiple times
                content = content.replace("null /* RejectedAt */, lr.CreatedAt, lr.CreatedAt))", "null /* RejectedAt */, lr.CreatedAt))")

                if content != orig_content:
                    with open(path, 'w', encoding='utf-8') as f:
                        f.write(content)
                    print(f"Updated {path}")

process_dir(dir_path)

print("Replacements done.")
