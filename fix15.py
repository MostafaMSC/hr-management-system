import os
import re

def fix15():
    # 1. LeaveRequestResponseDto CreatedAt
    files = [
        r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetPendingManagerApprovalsQueryHandler.cs",
        r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetPendingHRApprovalsQueryHandler.cs",
        r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetLeaveRequestByIdQueryHandler.cs",
        r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetEmployeeLeaveRequestsQueryHandler.cs"
    ]
    for p in files:
        with open(p, 'r', encoding='utf-8-sig') as f: c = f.read()
        # Replace avoiding duplicates
        c = re.sub(r'null\s*\/\*\s*RejectedAt\s*\*\/\s*\)\)', 'null /* RejectedAt */, lr.CreatedAt))', c)
        c = re.sub(r'null\s*\/\*\s*RejectedAt\s*\*\/\s*\)(?!\))', 'null /* RejectedAt */, lr.CreatedAt)', c)
        with open(p, 'w', encoding='utf-8') as f: f.write(c)

    # 2. AuthCommands.cs
    p = r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Auth\Commands\AuthCommands.cs"
    with open(p, 'r', encoding='utf-8-sig') as f: c = f.read()
    c = c.replace("user. cancellationToken);", "await _userRepository.UpdateAsync(user, cancellationToken);")
    with open(p, 'w', encoding='utf-8') as f: f.write(c)

    # 3 & 4. Sections
    files = [
        r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Sections\Commands\CreateSectionCommand.cs",
        r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Sections\Commands\UpdateSectionCommand.cs",
        r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Sections\Commands\DeleteSectionCommand.cs",
        r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Sections\Queries\GetSectionsQuery.cs"
    ]
    for p in files:
        with open(p, 'r', encoding='utf-8-sig') as f: c = f.read()
        c = re.sub(r'"Cache_"\.SectionsPrefix\(([^)]+)\)', r'($"Cache_SectionsPrefix_{\1}")', c)
        c = re.sub(r's\.CreatedAt\s*\?\?\s*DateTime\.UtcNow', 's.CreatedAt', c)
        c = re.sub(r's\.UpdatedAt\s*\?\?\s*DateTime\.UtcNow', 's.UpdatedAt', c)
        with open(p, 'w', encoding='utf-8') as f: f.write(c)

    # 5. CreateLeaveRequestCommandHandler.cs
    p = r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Commands\CreateLeaveRequestCommandHandler.cs"
    with open(p, 'r', encoding='utf-8-sig') as f: c = f.read()
    c = c.replace("EmployeeId = request.UserInfoId", "UserInfoId = request.UserInfoId")
    c = c.replace("LeaveReason = request.Reason", "Reason = request.Reason")
    c = c.replace("LeaveDate = request.StartDate", "StartDate = request.StartDate") 
    c = c.replace("StartTime = request.StartDate", "StartDate = request.StartDate")
    c = c.replace("EndTime = request.EndDate", "EndDate = request.EndDate")
    with open(p, 'w', encoding='utf-8') as f: f.write(c)

    # 6. UpdateUserDevicesCommand.cs
    p = r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Devices\Commands\UpdateUserDevicesCommand.cs"
    with open(p, 'r', encoding='utf-8-sig') as f: c = f.read()
    c = c.replace("var currentDeviceIds", "var currentDevices = user.UserDevices?.Select(ud => ud.Device).ToList() ?? new List<Device>();\n            var currentDeviceIds")
    with open(p, 'w', encoding='utf-8') as f: f.write(c)

fix15()
print("Fix 15 done.")
