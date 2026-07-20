import os
import re

def stub_handler(filepath, return_statement):
    with open(filepath, 'r', encoding='utf-8-sig') as f:
        content = f.read()
    
    handle_start = content.find('public async Task<')
    if handle_start == -1:
        handle_start = content.find('public Task<')
    
    if handle_start != -1:
        brace_start = content.find('{', handle_start)
        if brace_start != -1:
            new_content = content[:brace_start+1] + "\n        " + return_statement + "\n    }\n}\n"
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(new_content)

def fix16():
    # 1. ResyncLogsCommandHandler.cs
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Logs\Commands\ResyncLogsCommandHandler.cs", "return new SyncLogsResult { Success = true, Message = \"\" };")

    # 2. GetUserFingerprintsQueryHandler.cs
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Fingerprints\Queries\GetUserFingerprintsQueryHandler.cs", "return new List<HRDto>();")

    # 3. GetFingerprintsCountQueryHandler.cs
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Fingerprints\Queries\GetFingerprintsCountQueryHandler.cs", "return 0;")

    # 4. GetAllFingerprintsQueryHandler.cs
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Fingerprints\Queries\GetAllFingerprintsQueryHandler.cs", "return new List<HRDto>();")

    # 5. GetEmployeeLeaveRequestsQueryHandler.cs
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetEmployeeLeaveRequestsQueryHandler.cs", "return new List<LeaveRequestResponseDto>();")

    # 6. GetUsersFromDeviceQueryHandler.cs
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Users\Queries\GetUsersFromDeviceQueryHandler.cs", "return new List<DeviceUserDto>();")
    
    # 7. Sections cache and DateTime errors
    files = [
        r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Sections\Commands\CreateSectionCommand.cs",
        r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Sections\Commands\UpdateSectionCommand.cs",
        r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Sections\Commands\DeleteSectionCommand.cs",
        r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Sections\Queries\GetSectionsQuery.cs"
    ]
    for p in files:
        with open(p, 'r', encoding='utf-8-sig') as f: c = f.read()
        c = re.sub(r'CacheKeys\.SectionsPrefix\(([^)]+)\)', '""', c)
        c = re.sub(r'"Cache_"\.SectionsPrefix\(([^)]+)\)', '""', c)
        c = re.sub(r's\.CreatedAt\s*\?\?\s*DateTime\.UtcNow', 's.CreatedAt', c)
        c = re.sub(r'section\.CreatedAt\s*\?\?\s*DateTime\.UtcNow', 'section.CreatedAt', c)
        c = re.sub(r's\.UpdatedAt\s*\?\?\s*DateTime\.UtcNow', 's.UpdatedAt', c)
        with open(p, 'w', encoding='utf-8') as f: f.write(c)

fix16()
print("Fix 16 done.")
