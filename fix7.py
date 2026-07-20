import os
import re

dir_path = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application\\Attendance\\ZKPython"

replacements = [
    # GetEmployeeStatsQuery.cs
    (r'l\.UserID\s*==\s*BiometricId', 'l.UserID.ToString() == BiometricId'),
    (r'l\.UserID\s*==\s*username', 'l.UserID.ToString() == username'),
    
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
    (r'Role\s*=\s*req\.Role\s*\?\?\s*UserType\.User', 'Role = req.Role?.ToString() ?? "User"'),
    (r'Gender\s*=\s*req\.Gender,', 'Gender = req.Gender?.ToString(),'),
    (r'ShiftType\s*=\s*req\.ShiftType,', 'ShiftType = req.ShiftType?.ToString(),'),
    (r'AccountStatus\s*=\s*req\.AccountStatus\s*\?\?\s*AccountStatus\.Active', 'AccountStatus = req.AccountStatus?.ToString() ?? "Active"'),
    (r'PhoneNumber\s*=\s*phoneNumber,', 'PhoneNumber = phoneNumber?.ToString(),'),
    
    # EditUserCommandHandler.cs
    (r'req\.Role\s*\?\?\s*user\.Role', 'req.Role?.ToString() ?? user.Role'),
    (r'req\.Gender\s*\?\?\s*user\.Gender', 'req.Gender?.ToString() ?? user.Gender'),
    (r'req\.ShiftType\s*\?\?\s*user\.ShiftType', 'req.ShiftType?.ToString() ?? user.ShiftType'),
    (r'req\.AccountStatus\s*\?\?\s*user\.AccountStatus', 'req.AccountStatus?.ToString() ?? user.AccountStatus'),
    (r'PhoneNumber\s*=\s*phoneNumber;', 'user.PhoneNumber = phoneNumber?.ToString();'),
    (r'user\.PhoneNumber\s*=\s*phoneNumber;', 'user.PhoneNumber = phoneNumber?.ToString();'),

    # SyncUsersCommandHandler.cs
    (r'Role\s*=\s*UserType\.Employee', 'Role = "Employee"'),

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
