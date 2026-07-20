import os
import re

dir_path = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application\\Attendance\\ZKPython"
app_path = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application"

replacements = [
    # Revert GetValueOrDefault().Date back to .Date globally
    (r'\.GetValueOrDefault\(\)\.Date', '.Date'),

    # Enums in AddUserCommandHandler
    (r'UserType\s*=\s*request\.UserType,', 'Role = request.UserType.ToString(),'),
    (r'UserType\s*=\s*UserType\.Employee,', 'Role = "Employee",'),
    (r'Gender\s*=\s*request\.Gender,', 'Gender = request.Gender.ToString(),'),
    (r'ShiftType\s*=\s*request\.ShiftType,', 'ShiftType = request.ShiftType.ToString(),'),
    (r'AccountStatus\s*=\s*request\.AccountStatus,', 'AccountStatus = request.AccountStatus.ToString(),'),
    (r'AccountStatus\s*=\s*AccountStatus\.Active,', 'AccountStatus = "Active",'),
    
    # In EditUserCommandHandler (127,24)
    (r'UserType\s*=\s*request\.UserType\s*\?\?\s*currentUser\.Role,', 'Role = request.UserType?.ToString() ?? currentUser.Role,'),
    (r'Gender\s*=\s*request\.Gender\s*\?\?\s*currentUser\.Gender,', 'Gender = request.Gender?.ToString() ?? currentUser.Gender,'),
    (r'ShiftType\s*=\s*request\.ShiftType\s*\?\?\s*currentUser\.ShiftType,', 'ShiftType = request.ShiftType?.ToString() ?? currentUser.ShiftType,'),
    (r'AccountStatus\s*=\s*request\.AccountStatus\s*\?\?\s*currentUser\.AccountStatus,', 'AccountStatus = request.AccountStatus?.ToString() ?? currentUser.AccountStatus,'),

    # Other ?? string mappings
    (r'request\.Gender\s*\?\?\s*""', 'request.Gender?.ToString() ?? ""'),
    (r'request\.UserType\s*\?\?\s*""', 'request.UserType?.ToString() ?? ""'),
    (r'request\.ShiftType\s*\?\?\s*""', 'request.ShiftType?.ToString() ?? ""'),
    (r'request\.AccountStatus\s*\?\?\s*""', 'request.AccountStatus?.ToString() ?? ""'),

    # Fix string to int argument errors with BiometricId
    (r'\.GetByIdAsync\((biometricId|deviceUser)', r'.GetByBiometricIdAsync(\1'),
    (r'\.GetByIdAsync\(request\.BiometricId', r'.GetByBiometricIdAsync(request.BiometricId'),
    (r'\.GetByIdAsync\(userId', r'.GetByBiometricIdAsync(userId'), # SyncUsersCommandHandler (96,79) has userId from device which is string
    
    # Phone number assignment
    (r'PhoneNumber\s*=\s*request\.PhoneNumber,', 'PhoneNumber = request.PhoneNumber?.ToString(),'),
    (r'PhoneNumber\s*=\s*request\.PhoneNumber\s*\?\?\s*currentUser\.PhoneNumber,', 'PhoneNumber = request.PhoneNumber?.ToString() ?? currentUser.PhoneNumber,'),

    # == operator for enums
    (r'request\.LeaveType\s*==\s*LeaveType\.Sick', 'request.LeaveType.ToString() == "Sick"'),
    (r'request\.LeaveType\s*==\s*LeaveType\.Annual', 'request.LeaveType.ToString() == "Annual"'),
    (r'request\.LeaveType\s*==\s*LeaveType\.Emergency', 'request.LeaveType.ToString() == "Emergency"'),
    (r'request\.LeaveType\s*==\s*LeaveType\.Unpaid', 'request.LeaveType.ToString() == "Unpaid"'),
    (r'request\.LeaveType\s*==\s*LeaveType\.Maternity', 'request.LeaveType.ToString() == "Maternity"'),
    (r'request\.LeaveType\s*==\s*LeaveType\.Paternity', 'request.LeaveType.ToString() == "Paternity"'),
    
    # int and string ==
    (r'x\.UserInfoId\s*==\s*request\.BiometricId', 'x.BiometricId == request.BiometricId'),
    (r'request\.BiometricId\s*==\s*x\.UserInfoId', 'request.BiometricId == x.BiometricId'),
    (r'x\.UserInfoId\s*==\s*userId', 'x.BiometricId == userId'),
    (r'userId\s*==\s*x\.UserInfoId', 'userId == x.BiometricId'),
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
                
                # Fix ProcessDailySummariesCommand DateTime? Date
                if "ProcessDailySummariesCommand.cs" in file:
                    content = content.replace("log.PunchTime.Date", "log.PunchTime.Value.Date")
                    content = content.replace("log.PunchTime?.Date", "log.PunchTime?.Date")

                if content != orig_content:
                    with open(path, 'w', encoding='utf-8') as f:
                        f.write(content)
                    print(f"Updated {path}")

process_dir(dir_path)
process_dir(os.path.join(app_path, "Attendance", "Commands"))
process_dir(os.path.join(app_path, "Attendance", "Queries"))

print("Replacements done.")
