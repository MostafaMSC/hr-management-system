import os
import re

def fix13():
    # 1
    p = r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetLeaveBalanceQueryHandler.cs"
    with open(p, 'r', encoding='utf-8-sig') as f: c = f.read()
    c = re.sub(r'RemainingAnnualDays:\s*30,[\s\n]*\);', 'RemainingAnnualDays: 30);', c)
    with open(p, 'w', encoding='utf-8') as f: f.write(c)

    # 2
    p = r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Users\Commands\AddUserCommandHandler.cs"
    with open(p, 'r', encoding='utf-8-sig') as f: c = f.read()
    c = c.replace("newUser. cancellationToken);", "await _userRepository.CreateAsync(newUser, cancellationToken);")
    with open(p, 'w', encoding='utf-8') as f: f.write(c)

    # 3
    p = r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Users\Commands\DeleteUserCommandHandler.cs"
    with open(p, 'r', encoding='utf-8-sig') as f: c = f.read()
    c = c.replace('var  DeviceIp: {DeviceIp}", BiometricId, deviceIp);', 'var BiometricId = command.BiometricId;\n        var deviceIp = command.DeviceIp;\n        _logger.LogInformation("Deleting user with BiometricId: {BiometricId}, DeviceIp: {DeviceIp}", BiometricId, deviceIp);')
    with open(p, 'w', encoding='utf-8') as f: f.write(c)

    # 4
    p = r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Devices\Commands\UpdateUserDevicesCommand.cs"
    with open(p, 'r', encoding='utf-8-sig') as f: c = f.read()
    c = c.replace("""                user. cancellationToken);
            var currentDeviceIds""", """                user.BiometricId = user.Id.ToString();
                await _userRepository.UpdateAsync(user, cancellationToken);
            }
            var currentDeviceIds""")
    c = re.sub(r'Message\s*=\s*\$".*\{ex\.Message\}".*', 'Message = $"Error: {ex.Message}"\n            };\n        }\n    }\n}', c, flags=re.DOTALL)
    with open(p, 'w', encoding='utf-8') as f: f.write(c)

fix13()
print("Fix 13 done.")
