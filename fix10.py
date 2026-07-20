import os
import subprocess
import re

dir_path = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application"
files_to_restore = [
    "HR.Application/Attendance/ZKPython/Users/Commands/EditUserCommandHandler.cs",
    "HR.Application/Attendance/ZKPython/Users/Commands/SyncUsersCommandHandler.cs",
    "HR.Application/Attendance/ZKPython/Users/Queries/GetEmployeeStatsQuery.cs",
    "HR.Application/Attendance/ZKPython/Logs/Commands/SyncLogsCommandHandler.cs",
    "HR.Application/Attendance/ZKPython/Logs/Commands/ResyncLogsCommandHandler.cs"
]

for f in files_to_restore:
    cmd = ["git", "restore", f]
    subprocess.run(cmd, cwd="C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System")

replacements = [
    # GetEmployeeStatsQuery.cs
    (r'l\.UserID\s*==\s*BiometricId', 'l.UserInfoId == user.Id'),
    (r'l\.UserID\s*==\s*username', 'l.UserInfoId.ToString() == username'),
    (r'x\.LeaveType\s*==\s*LeaveType\.Sick', 'x.LeaveType == "Sick"'),
    (r'x\.LeaveType\s*==\s*LeaveType\.Annual', 'x.LeaveType == "Annual"'),
    (r'x\.LeaveType\s*==\s*LeaveType\.Emergency', 'x.LeaveType == "Emergency"'),
    (r'x\.LeaveType\s*==\s*LeaveType\.Unpaid', 'x.LeaveType == "Unpaid"'),
    (r'x\.LeaveType\s*==\s*LeaveType\.Maternity', 'x.LeaveType == "Maternity"'),
    (r'x\.LeaveType\s*==\s*LeaveType\.Paternity', 'x.LeaveType == "Paternity"'),

    # EditUserCommandHandler.cs
    (r'await\s*_userRepository\.GetByIdAsync\(request\.BiometricId\)', '(await _userRepository.GetAllUsersAsync(cancellationToken)).FirstOrDefault(u => u.BiometricId == request.BiometricId)'),
    (r'await\s*_userRepository\.GetByIdAsync\(BiometricId', '(await _userRepository.GetAllUsersAsync(cancellationToken)).FirstOrDefault(u => u.BiometricId == BiometricId'),
    (r'req\.Role\s*\?\?\s*user\.Role', 'req.Role?.ToString() ?? user.Role'),
    (r'req\.Gender\s*\?\?\s*user\.Gender', 'req.Gender?.ToString() ?? user.Gender'),
    (r'req\.ShiftType\s*\?\?\s*user\.ShiftType', 'req.ShiftType?.ToString() ?? user.ShiftType'),
    (r'req\.AccountStatus\s*\?\?\s*user\.AccountStatus', 'req.AccountStatus?.ToString() ?? user.AccountStatus'),
    (r'PhoneNumber\s*=\s*req\.PhoneNumber\s*\?\?\s*user\.PhoneNumber', 'user.PhoneNumber = req.PhoneNumber?.ToString() ?? user.PhoneNumber'),
    
    (r'Role\s*=\s*req\.Role\s*\?\?\s*UserType\.User,', 'Role = req.Role?.ToString() ?? "User",'),
    (r'Gender\s*=\s*req\.Gender,', 'Gender = req.Gender?.ToString(),'),
    (r'ShiftType\s*=\s*req\.ShiftType,', 'ShiftType = req.ShiftType?.ToString(),'),
    (r'AccountStatus\s*=\s*req\.AccountStatus\s*\?\?\s*AccountStatus\.Active,', 'AccountStatus = req.AccountStatus?.ToString() ?? "Active",'),
    (r'PhoneNumber\s*=\s*phoneNumber,', 'PhoneNumber = phoneNumber?.ToString(),'),
    (r'user\.PhoneNumber\s*=\s*phoneNumber;', 'user.PhoneNumber = phoneNumber?.ToString();'),

    # SyncUsersCommandHandler.cs
    (r'await\s*_userRepository\.GetByIdAsync\(userId\)', '(await _userRepository.GetAllUsersAsync(cancellationToken)).FirstOrDefault(u => u.BiometricId == userId)'),
    (r'await\s*_userRepository\.GetByIdAsync\(userId,\s*cancellationToken\)', '(await _userRepository.GetAllUsersAsync(cancellationToken)).FirstOrDefault(u => u.BiometricId == userId)'),
    (r'UserType\s*=\s*UserType\.Employee', 'Role = "Employee"'),
    (r'Role\s*=\s*targetRole', 'Role = targetRole.ToString()'),

    # SyncLogsCommandHandler.cs & ResyncLogsCommandHandler.cs
    (r'UserID\s*=\s*log\.UserId,', 'UserInfoId = user.Id,'),
    (r'Name\s*=\s*log\.Name,', ''),
    (r'Card\s*=\s*log\.Card,', ''),
    (r'Role\s*=\s*log\.Role,', ''),
    (r'DeviceIP\s*=\s*log\.DeviceIP,', ''),
]

for f in files_to_restore:
    path = os.path.join("C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System", f)
    with open(path, 'r', encoding='utf-8-sig') as file:
        content = file.read()
        
    orig_content = content
    for pattern, repl in replacements:
        content = re.sub(pattern, repl, content)
    
    if content != orig_content:
        with open(path, 'w', encoding='utf-8') as file:
            file.write(content)
        print(f"Updated {f}")

print("Fix 10 done.")
