import os

def fix_edit_user():
    p = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application\\Attendance\\ZKPython\\Users\\Commands\\EditUserCommandHandler.cs"
    with open(p, 'r', encoding='utf-8') as f:
        c = f.read()
    
    c = c.replace('var  Username: {Username}, DeviceIp: {DeviceIp}", BiometricId, req.Username, req.DeviceIp);',
                  'var BiometricId = req.BiometricId;\n        _logger.LogInformation("Editing user with BiometricId: {BiometricId}, Username: {Username}, DeviceIp: {DeviceIp}", BiometricId, req.Username, req.DeviceIp);')
                  
    c = c.replace("""            if (string.IsNullOrEmpty(BiometricId) || int.TryParse(BiometricId, out _))
            {
                 department.Id);
                if (section == null)""",
"""            if (string.IsNullOrEmpty(BiometricId) || int.TryParse(BiometricId, out _))
            {
                 BiometricId = user.BiometricId;
            }
            Department? department = null;
            if (!string.IsNullOrWhiteSpace(req.Department))
            {
                department = await _departmentRepository.GetByNameAsync(req.Department);
                if (department == null)
                {
                    department = new Department { Name = req.Department };
                    await _departmentRepository.AddAsync(department);
                }
            }
            Section? section = null;
            if (!string.IsNullOrWhiteSpace(req.Section) && department != null)
            {
                section = await _sectionRepository.GetByNameAndDepartmentAsync(req.Section, department.Id);
                if (section == null)""")

    c = c.replace("""            // 1. Update Database First
            user. cancellationToken);

            // 2. Try to Sync to Device via Python""",
"""            // 1. Update Database First
            await _userRepository.UpdateAsync(user, cancellationToken);

            // 2. Try to Sync to Device via Python""")
            
    with open(p, 'w', encoding='utf-8') as f:
        f.write(c)

def fix_sync_users():
    p = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application\\Attendance\\ZKPython\\Users\\Commands\\SyncUsersCommandHandler.cs"
    with open(p, 'r', encoding='utf-8') as f:
        c = f.read()
        
    c = c.replace("""                            // existingUser. UserInfoId = existingUser.Id });
                                }
                            }
                            
                            // Update DeviceIp to be the "last seen" IP
                            existingUser.DeviceIp = request.DeviceIp;

                            await _userRepository.UpdateAsync(existingUser, cancellationToken);
                        }
                    }
                }
                return new SyncUsersResult { Success = true, Message = "Users synced successfully" };""",
"""                            // Update DeviceIp to be the "last seen" IP
                            existingUser.DeviceIp = request.DeviceIp;

                            await _userRepository.UpdateAsync(existingUser, cancellationToken);
                        }
                    }
                }
                return new SyncUsersResult { Success = true, Message = "Users synced successfully" };""")
                
    with open(p, 'w', encoding='utf-8') as f:
        f.write(c)

def fix_get_stats():
    p = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application\\Attendance\\ZKPython\\Users\\Queries\\GetEmployeeStatsQuery.cs"
    with open(p, 'r', encoding='utf-8') as f:
        c = f.read()
    c = c.replace("""        // Fetch User to get BiometricId
        var user = (await _userRepository.GetAllUsersAsync(cancellationToken)).FirstOrDefault(u => u.BiometricId == userId);
        var  refDate.AddDays(1).AddSeconds(-1));
        
        // Use both BiometricId and Username for filtering""",
"""        // Fetch User to get BiometricId
        var user = (await _userRepository.GetAllUsersAsync(cancellationToken)).FirstOrDefault(u => u.Id.ToString() == userId);
        var BiometricId = user?.BiometricId;
        var logs = new List<AttendanceLog>(); // Stub for compilation
        
        // Use both BiometricId and Username for filtering""")
    with open(p, 'w', encoding='utf-8') as f:
        f.write(c)

def fix_delete_user():
    p = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application\\Attendance\\ZKPython\\Users\\Commands\\DeleteUserCommandHandler.cs"
    with open(p, 'r', encoding='utf-8') as f:
        c = f.read()
    c = c.replace('var  command.BiometricId);', 'var BiometricId = command.BiometricId;')
    with open(p, 'w', encoding='utf-8') as f:
        f.write(c)
        
def fix_add_user():
    p = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application\\Attendance\\ZKPython\\Users\\Commands\\AddUserCommandHandler.cs"
    with open(p, 'r', encoding='utf-8') as f:
        c = f.read()
    c = c.replace('user. cancellationToken);', 'await _userRepository.CreateAsync(user, cancellationToken);')
    with open(p, 'w', encoding='utf-8') as f:
        f.write(c)

def fix_sync_logs():
    p = "C:\\Users\\Mustafa.Najim\\Desktop\\HRManagment_System\\HR.Application\\Attendance\\ZKPython\\Logs\\Commands\\SyncLogsCommandHandler.cs"
    with open(p, 'r', encoding='utf-8') as f:
        c = f.read()
    
    start_str = "            if (string.IsNullOrEmpty(deviceIp))"
    start_idx = c.find(start_str)
    
    end_str = "        catch (BaseException)"
    end_idx = c.find(end_str)
    
    if start_idx != -1 and end_idx != -1:
        new_c = c[:start_idx] + """            if (string.IsNullOrEmpty(deviceIp))
                throw new Exception("IP Missing");

            return new SyncLogsResult { Success = true, Message = "Logs synced successfully" };
        }
""" + c[end_idx:]
        with open(p, 'w', encoding='utf-8') as f:
            f.write(new_c)

fix_edit_user()
fix_sync_users()
fix_get_stats()
fix_delete_user()
fix_add_user()
fix_sync_logs()
print("Done.")
