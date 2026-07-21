using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HR.Infrastructure.Data.Seeders;

public static class MockDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        var rand = new Random(42);

        // 1. Seed Shifts
        if (!await context.AttendanceShifts.AnyAsync())
        {
            var standardShift = new AttendanceShift { Name = "Standard Shift", StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), LateThreshold = TimeSpan.FromMinutes(15), IsDeleted = false };
            var nightShift = new AttendanceShift { Name = "Night Shift", StartTime = new TimeSpan(22, 0, 0), EndTime = new TimeSpan(6, 0, 0), LateThreshold = TimeSpan.FromMinutes(15), IsDeleted = false };
            context.AttendanceShifts.AddRange(standardShift, nightShift);
            await context.SaveChangesAsync();
        }

        // 2. Seed Departments
        if (!await context.Departments.AnyAsync())
        {
            var itDept = new Department { Name = "IT Department", Description = "Information Technology", IsDeleted = false };
            var hrDept = new Department { Name = "HR Department", Description = "Human Resources", IsDeleted = false };
            var salesDept = new Department { Name = "Sales Department", Description = "Sales and Marketing", IsDeleted = false };
            context.Departments.AddRange(itDept, hrDept, salesDept);
            await context.SaveChangesAsync();
        }

        // 3. Seed Sections
        if (!await context.Sections.AnyAsync())
        {
            var itDept = await context.Departments.FirstAsync(d => d.Name == "IT Department");
            var hrDept = await context.Departments.FirstAsync(d => d.Name == "HR Department");
            
            var frontendSection = new Section { Name = "Frontend Development", DepartmentId = itDept.Id, IsDeleted = false };
            var backendSection = new Section { Name = "Backend Development", DepartmentId = itDept.Id, IsDeleted = false };
            var recruitmentSection = new Section { Name = "Recruitment", DepartmentId = hrDept.Id, IsDeleted = false };
            var payrollSection = new Section { Name = "Payroll", DepartmentId = hrDept.Id, IsDeleted = false };
            context.Sections.AddRange(frontendSection, backendSection, recruitmentSection, payrollSection);
            await context.SaveChangesAsync();
        }

        // 4. Seed Devices
        if (!await context.Devices.AnyAsync())
        {
            var mainDevice = new Device { Name = "Main Entrance Device", DeviceName = "ZKTeco K40", IpAddress = "192.168.1.201", Port = 4370, Protocol = DeviceProtocol.ZkTecoTcp, IsActive = true, SerialNumber = "SN123456" };
            var backDevice = new Device { Name = "Back Entrance Device", DeviceName = "ZKTeco UFace", IpAddress = "192.168.1.202", Port = 4370, Protocol = DeviceProtocol.ZkTecoTcp, IsActive = true, SerialNumber = "SN654321" };
            context.Devices.AddRange(mainDevice, backDevice);
            await context.SaveChangesAsync();
        }

        // 5. Seed Holidays
        if (!await context.Holidays.AnyAsync())
        {
            var newYear = new Holiday { Name = "New Year", Date = new DateTime(DateTime.UtcNow.Year + 1, 1, 1), Description = "New Year Holiday" };
            var nationalDay = new Holiday { Name = "National Day", Date = new DateTime(DateTime.UtcNow.Year, 10, 1), Description = "National Day Celebration" };
            context.Holidays.AddRange(newYear, nationalDay);
            await context.SaveChangesAsync();
        }

        // 6. Seed Users
        var userCount = await context.UserInfos.CountAsync();
        if (userCount < 100)
        {
            var itDept = await context.Departments.FirstAsync(d => d.Name == "IT Department");
            var hrDept = await context.Departments.FirstAsync(d => d.Name == "HR Department");
            var salesDept = await context.Departments.FirstAsync(d => d.Name == "Sales Department");
            
            var frontendSection = await context.Sections.FirstAsync(s => s.Name == "Frontend Development");
            var backendSection = await context.Sections.FirstAsync(s => s.Name == "Backend Development");
            var recruitmentSection = await context.Sections.FirstAsync(s => s.Name == "Recruitment");
            var payrollSection = await context.Sections.FirstAsync(s => s.Name == "Payroll");

            var standardShift = await context.AttendanceShifts.FirstAsync(s => s.Name == "Standard Shift");
            var nightShift = await context.AttendanceShifts.FirstAsync(s => s.Name == "Night Shift");

            if (userCount == 0)
            {
                var users = new List<UserInfo>();
                var itManager = new UserInfo
                {
                    Username = "itmanager", Email = "itmanager@company.com", PasswordHash = passwordHasher.HashPassword("Password123!"),
                    FirstName = "IT", LastName = "Manager", Role = UserType.Manager, DepartmentId = itDept.Id, AttendanceShiftId = standardShift.Id,
                    DateOfJoining = DateTime.UtcNow.AddYears(-5), IsDeleted = false, AccountStatus = "Active"
                };
                var hrManager = new UserInfo
                {
                    Username = "hrmanager", Email = "hrmanager@company.com", PasswordHash = passwordHasher.HashPassword("Password123!"),
                    FirstName = "HR", LastName = "Manager", Role = UserType.HR, DepartmentId = hrDept.Id, AttendanceShiftId = standardShift.Id,
                    DateOfJoining = DateTime.UtcNow.AddYears(-4), IsDeleted = false, AccountStatus = "Active"
                };
                users.AddRange(new[] { itManager, hrManager });
                context.UserInfos.AddRange(users);
                await context.SaveChangesAsync();

                itDept.ManagerId = itManager.Id;
                hrDept.ManagerId = hrManager.Id;
                await context.SaveChangesAsync();
                
                userCount = 2;
            }

            var itManagerExisting = await context.UserInfos.FirstOrDefaultAsync(u => u.Username == "itmanager");
            var hrManagerExisting = await context.UserInfos.FirstOrDefaultAsync(u => u.Username == "hrmanager");

            var firstNames = new[] { "Ahmed", "Mohammed", "Fatima", "Ali", "Omar", "Sara", "Noor", "Hassan", "Zainab", "Yousef", "Aisha", "Khalid", "Tariq", "Huda", "Mona", "Layla" };
            var lastNames = new[] { "Al-Sayed", "Hassan", "Ali", "Mahmoud", "Abdullah", "Ibrahim", "Yassin", "Sami", "Farooq", "Mansour", "Khalil", "Nasser" };
            var generatedUsers = new List<UserInfo>();

            var maxEmpId = await context.UserInfos.Where(u => u.Username.StartsWith("emp")).CountAsync();
            var neededUsers = 100 - userCount;

            for (int i = maxEmpId + 1; i <= maxEmpId + neededUsers; i++)
            {
                var dept = rand.Next(1, 4) switch { 1 => itDept, 2 => hrDept, _ => salesDept };
                var isIt = dept.Id == itDept.Id;
                var isHr = dept.Id == hrDept.Id;
                int? sectionId = null;
                if (isIt) sectionId = rand.Next(1, 3) == 1 ? frontendSection.Id : backendSection.Id;
                if (isHr) sectionId = rand.Next(1, 3) == 1 ? recruitmentSection.Id : payrollSection.Id;
                int? managerId = isIt ? itManagerExisting?.Id : (isHr ? hrManagerExisting?.Id : null);

                generatedUsers.Add(new UserInfo
                {
                    Username = $"emp{i}", Email = $"emp{i}@company.com", PasswordHash = passwordHasher.HashPassword("Password123!"),
                    FirstName = firstNames[rand.Next(firstNames.Length)], LastName = lastNames[rand.Next(lastNames.Length)],
                    Role = UserType.Employee, DepartmentId = dept.Id, SectionId = sectionId, DirectManagerId = managerId,
                    AttendanceShiftId = rand.Next(1, 10) > 8 ? nightShift.Id : standardShift.Id,
                    DateOfJoining = DateTime.UtcNow.AddDays(-rand.Next(100, 1000)), IsDeleted = false, AccountStatus = "Active",
                    BiometricId = (1000 + i).ToString()
                });
            }
            context.UserInfos.AddRange(generatedUsers);
            await context.SaveChangesAsync();
        }

        var allEmployees = await context.UserInfos.ToListAsync();

        // 7. Seed Attendance Logs
        var existingLogUserIds = await context.AttendanceLogs.Select(l => l.UserInfoId).Distinct().ToListAsync();
        var usersWithoutLogs = allEmployees.Where(u => !existingLogUserIds.Contains(u.Id)).ToList();
        
        if (usersWithoutLogs.Any())
        {
            var logs = new List<AttendanceLog>();
            var endDate = DateTime.UtcNow.Date;
            var startDate = endDate.AddDays(-60); // 60 days of logs for more data

            foreach (var emp in usersWithoutLogs)
            {
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    if (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday) continue;
                    if (rand.Next(100) < 90) // 90% chance to be present
                    {
                        var checkInTime = date.AddHours(9).AddMinutes(rand.Next(-15, 30));
                        var checkOutTime = date.AddHours(17).AddMinutes(rand.Next(0, 60));

                        logs.Add(new AttendanceLog { UserInfoId = emp.Id, PunchTime = checkInTime, PunchType = PunchType.CheckIn, CreatedAt = checkInTime });
                        logs.Add(new AttendanceLog { UserInfoId = emp.Id, PunchTime = checkOutTime, PunchType = PunchType.CheckOut, CreatedAt = checkOutTime });
                    }
                }
            }
            context.AttendanceLogs.AddRange(logs);
            await context.SaveChangesAsync();
        }

        // 8. Seed Leave Requests
        if (!await context.LeaveRequests.AnyAsync() && allEmployees.Any())
        {
            var leaveRequests = new List<LeaveRequest>();
            var leaveTypes = new[] { LeaveType.Sick, LeaveType.Personal, LeaveType.Unpaid, LeaveType.WorkFromHome };
            var statuses = new[] { LeaveStatus.Pending, LeaveStatus.Approved, LeaveStatus.Rejected };

            for (int i = 0; i < 40; i++)
            {
                var emp = allEmployees[rand.Next(allEmployees.Count)];
                var start = DateTime.UtcNow.AddDays(rand.Next(-30, 30));
                leaveRequests.Add(new LeaveRequest
                {
                    UserInfoId = emp.Id, LeaveType = leaveTypes[rand.Next(leaveTypes.Length)], StartDate = start, EndDate = start.AddDays(rand.Next(1, 5)),
                    Status = statuses[rand.Next(statuses.Length)], Reason = "Personal reasons", CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(1, 10))
                });
            }
            context.LeaveRequests.AddRange(leaveRequests);
            await context.SaveChangesAsync();
        }

        // 9. Seed Bonus Requests
        if (!await context.BonusRequests.AnyAsync() && allEmployees.Any())
        {
            var bonusRequests = new List<BonusRequest>();
            var bonusStatuses = new[] { BonusStatus.Pending, BonusStatus.Approved, BonusStatus.Rejected };
            var managers = allEmployees.Where(e => e.Role == UserType.Manager || e.Role == UserType.HR).ToList();

            for (int i = 0; i < 20; i++)
            {
                var emp = allEmployees[rand.Next(allEmployees.Count)];
                var mgr = managers.FirstOrDefault(m => m.Id == emp.DirectManagerId) ?? managers.FirstOrDefault();
                if (mgr != null)
                {
                    bonusRequests.Add(new BonusRequest
                    {
                        RequestingManagerId = mgr.Id, TargetUserId = emp.Id, Value = rand.Next(100, 1000), Type = BonusType.Amount,
                        Reason = "Outstanding performance", Status = bonusStatuses[rand.Next(bonusStatuses.Length)],
                        Year = DateTime.UtcNow.Year, Month = DateTime.UtcNow.Month, IsDeleted = false, CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(1, 10))
                    });
                }
            }
            context.BonusRequests.AddRange(bonusRequests);
            await context.SaveChangesAsync();
        }

        // 10. Seed Notifications
        if (!await context.Notifications.AnyAsync() && allEmployees.Any())
        {
            var notifications = new List<Notification>();
            var pendingLeaves = await context.LeaveRequests.Where(r => r.Status == LeaveStatus.Pending).ToListAsync();
            foreach (var req in pendingLeaves)
            {
                var emp = allEmployees.FirstOrDefault(u => u.Id == req.UserInfoId);
                if (emp?.DirectManagerId.HasValue == true)
                {
                    notifications.Add(new Notification
                    {
                        UserId = emp.DirectManagerId.Value, Title = "New Leave Request", Message = $"User {req.UserInfoId} requested time off.",
                        Type = NotificationType.RequestPending, IsRead = false, Data = req.Id.ToString(), CreatedAt = DateTime.UtcNow
                    });
                }
            }
            context.Notifications.AddRange(notifications);
            await context.SaveChangesAsync();
        }

        // Fetch devices for relations
        var dbMainDevice = await context.Devices.FirstOrDefaultAsync() ?? new Device { Id = 1 };
        var dbBackDevice = await context.Devices.Skip(1).FirstOrDefaultAsync() ?? dbMainDevice;

        // 11. Seed UserDevices
        var existingDeviceUserIds = await context.UserDevices.Select(u => u.UserInfoId).Distinct().ToListAsync();
        var usersWithoutDevices = allEmployees.Where(u => !existingDeviceUserIds.Contains(u.Id)).ToList();
        
        if (usersWithoutDevices.Any() && await context.Devices.AnyAsync())
        {
            var userDevices = new List<UserDevice>();
            foreach (var emp in usersWithoutDevices)
            {
                userDevices.Add(new UserDevice { UserInfoId = emp.Id, DeviceId = dbMainDevice.Id, ZkEnrollNumber = emp.BiometricId ?? emp.Id.ToString(), CreatedAt = DateTime.UtcNow });
                if (rand.Next(2) == 0 && dbBackDevice.Id != dbMainDevice.Id)
                {
                    userDevices.Add(new UserDevice { UserInfoId = emp.Id, DeviceId = dbBackDevice.Id, ZkEnrollNumber = emp.BiometricId ?? emp.Id.ToString(), CreatedAt = DateTime.UtcNow });
                }
            }
            context.UserDevices.AddRange(userDevices);
            await context.SaveChangesAsync();
        }

        // 12. Seed LeaveBalances
        var existingLeaveBalanceUserIds = await context.LeaveBalances.Select(l => l.UserInfoId).Distinct().ToListAsync();
        var usersWithoutLeaveBalances = allEmployees.Where(u => !existingLeaveBalanceUserIds.Contains(u.Id)).ToList();
        
        if (usersWithoutLeaveBalances.Any())
        {
            var leaveBalances = new List<LeaveBalance>();
            var currentYear = DateTime.UtcNow.Year;
            foreach (var emp in usersWithoutLeaveBalances)
            {
                leaveBalances.Add(new LeaveBalance { UserInfoId = emp.Id, Year = currentYear, LeaveType = "Annual", TotalAllowed = 21, Used = rand.Next(0, 10), CreatedAt = DateTime.UtcNow });
            }
            context.LeaveBalances.AddRange(leaveBalances);
            await context.SaveChangesAsync();
        }

        // 13. Seed DailyAttendanceSummaries
        if (!await context.DailyAttendanceSummaries.AnyAsync() && allEmployees.Any())
        {
            var summaries = new List<DailyAttendanceSummary>();
            var endDate = DateTime.UtcNow.Date;
            var summaryStartDate = endDate.AddDays(-14); // Extended to 14 days
            foreach (var emp in allEmployees)
            {
                for (var date = summaryStartDate; date <= endDate; date = date.AddDays(1))
                {
                    if (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday) continue;
                    
                    summaries.Add(new DailyAttendanceSummary
                    {
                        UserInfoId = emp.Id, Date = date, TimeIn = new TimeSpan(9, rand.Next(0, 30), 0), TimeOut = new TimeSpan(17, rand.Next(0, 30), 0),
                        DelayMinutes = rand.Next(0, 15), OvertimeMinutes = rand.Next(0, 60), Status = AttendanceStatus.Regular, IsDeductionApplied = false, CreatedAt = DateTime.UtcNow
                    });
                }
            }
            context.DailyAttendanceSummaries.AddRange(summaries);
            await context.SaveChangesAsync();
        }

        // 14. Seed Fingerprints
        var existingFingerprintUserIds = await context.Fingerprints.Select(f => f.UserId).Distinct().ToListAsync();
        var usersWithoutFingerprints = allEmployees.Where(u => !existingFingerprintUserIds.Contains(u.Id)).ToList();
        
        if (usersWithoutFingerprints.Any())
        {
            var fingerprints = new List<Fingerprint>();
            foreach (var emp in usersWithoutFingerprints)
            {
                fingerprints.Add(new Fingerprint
                {
                    UserId = emp.Id, DeviceUserId = emp.BiometricId ?? emp.Id.ToString(), Username = emp.Username, FingerIndex = 1,
                    Template = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, TemplateSize = 10, DeviceIp = dbMainDevice.IpAddress, IsValid = true, CreatedAt = DateTime.UtcNow
                });
            }
            context.Fingerprints.AddRange(fingerprints);
            await context.SaveChangesAsync();
        }
    }
}
