import os
import re

dir_path = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application\\Attendance\\ZKPython"

replacements = [
    # GetEmployeeStatsQuery.cs
    (r'l\.UserInfoId\s*==\s*BiometricId', 'l.UserInfoId.ToString() == BiometricId'),
    (r'l\.UserInfoId\s*==\s*username', 'l.UserInfoId.ToString() == username'),
    
    (r'x\.LeaveType\s*==\s*LeaveType\.Sick', 'x.LeaveType == "Sick"'),
    (r'x\.LeaveType\s*==\s*LeaveType\.Annual', 'x.LeaveType == "Annual"'),
    (r'x\.LeaveType\s*==\s*LeaveType\.Emergency', 'x.LeaveType == "Emergency"'),
    (r'x\.LeaveType\s*==\s*LeaveType\.Unpaid', 'x.LeaveType == "Unpaid"'),
    (r'x\.LeaveType\s*==\s*LeaveType\.Maternity', 'x.LeaveType == "Maternity"'),
    (r'x\.LeaveType\s*==\s*LeaveType\.Paternity', 'x.LeaveType == "Paternity"'),

    # DeleteUserCommandHandler.cs & EditUserCommandHandler.cs & SyncUsersCommandHandler.cs
    (r'await\s*_userRepository\.GetByIdAsync\(request\.BiometricId\)', '(await _userRepository.GetAllUsersAsync(cancellationToken)).FirstOrDefault(u => u.BiometricId == request.BiometricId)'),
    (r'await\s*_userRepository\.GetByIdAsync\(userId\)', '(await _userRepository.GetAllUsersAsync(cancellationToken)).FirstOrDefault(u => u.BiometricId == userId)'),
    
    # AddUserCommandHandler.cs
    (r'UserType\s*=\s*request\.UserType', 'Role = request.UserType.ToString()'),
    (r'Gender\s*=\s*request\.Gender', 'Gender = request.Gender.ToString()'),
    (r'ShiftType\s*=\s*request\.ShiftType', 'ShiftType = request.ShiftType.ToString()'),
    (r'AccountStatus\s*=\s*request\.AccountStatus', 'AccountStatus = request.AccountStatus.ToString()'),
    (r'PhoneNumber\s*=\s*request\.PhoneNumber', 'PhoneNumber = request.PhoneNumber?.ToString()'),
    
    # EditUserCommandHandler.cs
    (r'request\.UserType\s*\?\?\s*currentUser\.Role', 'request.UserType?.ToString() ?? currentUser.Role'),
    (r'request\.Gender\s*\?\?\s*currentUser\.Gender', 'request.Gender?.ToString() ?? currentUser.Gender'),
    (r'request\.ShiftType\s*\?\?\s*currentUser\.ShiftType', 'request.ShiftType?.ToString() ?? currentUser.ShiftType'),
    (r'request\.AccountStatus\s*\?\?\s*currentUser\.AccountStatus', 'request.AccountStatus?.ToString() ?? currentUser.AccountStatus'),
    (r'request\.PhoneNumber\s*\?\?\s*currentUser\.PhoneNumber', 'request.PhoneNumber?.ToString() ?? currentUser.PhoneNumber'),

    # SyncUsersCommandHandler.cs
    (r'UserType\s*=\s*UserType\.Employee', 'Role = "Employee"'),

    # UpdateUserDevicesCommand.cs
    (r'\.GetByBiometricIdAndIndexAsync', '.GetByUserIdAndFingerIndexAsync'),
    (r'new\s+HR\(\s*\)', 'new UserDevice()'),
    
    # GetWeeklyLateQuery.cs
    (r'BiometricId\s*=\s*x\.UserInfoId,', 'BiometricId = x.UserInfoId.ToString(),'),
    
    # ApproveLeaveByManagerCommandHandler.cs
    (r'department\.ManagerId', '0'),
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
