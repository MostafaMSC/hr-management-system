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
        // Seed if the mock 'IT Department' doesn't exist yet
        if (await context.Departments.AnyAsync(d => d.Name == "IT Department")) return;

        var rand = new Random(42); // Deterministic seed for reproducible testing
        
        // 1. Seed Shifts
        var standardShift = new AttendanceShift { Name = "Standard Shift", StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), LateThreshold = TimeSpan.FromMinutes(15), IsDeleted = false };
        var nightShift = new AttendanceShift { Name = "Night Shift", StartTime = new TimeSpan(22, 0, 0), EndTime = new TimeSpan(6, 0, 0), LateThreshold = TimeSpan.FromMinutes(15), IsDeleted = false };
        context.AttendanceShifts.AddRange(standardShift, nightShift);
        await context.SaveChangesAsync();

        // 2. Seed Departments
        var itDept = new Department { Name = "IT Department", Description = "Information Technology", IsDeleted = false };
        var hrDept = new Department { Name = "HR Department", Description = "Human Resources", IsDeleted = false };
        var salesDept = new Department { Name = "Sales Department", Description = "Sales and Marketing", IsDeleted = false };
        context.Departments.AddRange(itDept, hrDept, salesDept);
        await context.SaveChangesAsync();

        // 3. Seed Sections
        var frontendSection = new Section { Name = "Frontend Development", DepartmentId = itDept.Id, IsDeleted = false };
        var backendSection = new Section { Name = "Backend Development", DepartmentId = itDept.Id, IsDeleted = false };
        var recruitmentSection = new Section { Name = "Recruitment", DepartmentId = hrDept.Id, IsDeleted = false };
        var payrollSection = new Section { Name = "Payroll", DepartmentId = hrDept.Id, IsDeleted = false };
        context.Sections.AddRange(frontendSection, backendSection, recruitmentSection, payrollSection);
        await context.SaveChangesAsync();

        // 4. Seed Devices
        var mainDevice = new Device { Name = "Main Entrance Device", DeviceName = "ZKTeco K40", IpAddress = "192.168.1.201", Port = 4370, Protocol = DeviceProtocol.ZkTecoTcp, IsActive = true, SerialNumber = "SN123456" };
        var backDevice = new Device { Name = "Back Entrance Device", DeviceName = "ZKTeco UFace", IpAddress = "192.168.1.202", Port = 4370, Protocol = DeviceProtocol.ZkTecoTcp, IsActive = true, SerialNumber = "SN654321" };
        context.Devices.AddRange(mainDevice, backDevice);
        await context.SaveChangesAsync();

        // 5. Seed Holidays
        var newYear = new Holiday { Name = "New Year", Date = new DateTime(DateTime.UtcNow.Year + 1, 1, 1), Description = "New Year Holiday" };
        var nationalDay = new Holiday { Name = "National Day", Date = new DateTime(DateTime.UtcNow.Year, 10, 1), Description = "National Day Celebration" };
        context.Holidays.AddRange(newYear, nationalDay);
        await context.SaveChangesAsync();

        // 6. Seed Users
        var users = new List<UserInfo>();

        // Create IT Manager
        var itManager = new UserInfo
        {
            Username = "itmanager",
            Email = "itmanager@company.com",
            PasswordHash = passwordHasher.HashPassword("Password123!"),
            FirstName = "IT",
            LastName = "Manager",
            Role = UserType.Manager,
            DepartmentId = itDept.Id,
            AttendanceShiftId = standardShift.Id,
            DateOfJoining = DateTime.UtcNow.AddYears(-5),
            IsDeleted = false,
            AccountStatus = "Active"
        };
        users.Add(itManager);

        // Create HR Manager
        var hrManager = new UserInfo
        {
            Username = "hrmanager",
            Email = "hrmanager@company.com",
            PasswordHash = passwordHasher.HashPassword("Password123!"),
            FirstName = "HR",
            LastName = "Manager",
            Role = UserType.HR,
            DepartmentId = hrDept.Id,
            AttendanceShiftId = standardShift.Id,
            DateOfJoining = DateTime.UtcNow.AddYears(-4),
            IsDeleted = false,
            AccountStatus = "Active"
        };
        users.Add(hrManager);

        context.UserInfos.AddRange(users);
        await context.SaveChangesAsync(); // Save managers to get IDs

        // Update Departments with Managers
        itDept.ManagerId = itManager.Id;
        hrDept.ManagerId = hrManager.Id;
        await context.SaveChangesAsync();

        // Generate 48 Employees
        var firstNames = new[] { "Ahmed", "Mohammed", "Fatima", "Ali", "Omar", "Sara", "Noor", "Hassan", "Zainab", "Yousef", "Aisha", "Khalid" };
        var lastNames = new[] { "Al-Sayed", "Hassan", "Ali", "Mahmoud", "Abdullah", "Ibrahim", "Yassin", "Sami", "Farooq", "Mansour" };

        var generatedUsers = new List<UserInfo>();
        for (int i = 1; i <= 48; i++)
        {
            var dept = rand.Next(1, 4) switch { 1 => itDept, 2 => hrDept, _ => salesDept };
            var isIt = dept.Id == itDept.Id;
            var isHr = dept.Id == hrDept.Id;
            
            int? sectionId = null;
            if (isIt) sectionId = rand.Next(1, 3) == 1 ? frontendSection.Id : backendSection.Id;
            if (isHr) sectionId = rand.Next(1, 3) == 1 ? recruitmentSection.Id : payrollSection.Id;

            int? managerId = isIt ? itManager.Id : (isHr ? hrManager.Id : null);

            var fname = firstNames[rand.Next(firstNames.Length)];
            var lname = lastNames[rand.Next(lastNames.Length)];
            var shift = rand.Next(1, 10) > 8 ? nightShift : standardShift;

            generatedUsers.Add(new UserInfo
            {
                Username = $"emp{i}",
                Email = $"emp{i}@company.com",
                PasswordHash = passwordHasher.HashPassword("Password123!"),
                FirstName = fname,
                LastName = lname,
                Role = UserType.Employee,
                DepartmentId = dept.Id,
                SectionId = sectionId,
                DirectManagerId = managerId,
                AttendanceShiftId = shift.Id,
                DateOfJoining = DateTime.UtcNow.AddDays(-rand.Next(100, 1000)),
                IsDeleted = false,
                AccountStatus = "Active",
                BiometricId = (1000 + i).ToString()
            });
        }
        context.UserInfos.AddRange(generatedUsers);
        await context.SaveChangesAsync();

        var allEmployees = generatedUsers.ToList();

        // 7. Seed Attendance Logs
        var logs = new List<AttendanceLog>();
        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-30);

        foreach (var emp in allEmployees)
        {
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday) continue;
                
                // 90% chance to be present
                if (rand.Next(100) < 90)
                {
                    // Check in between 8:45 and 9:30
                    var checkInMin = rand.Next(-15, 30);
                    var checkInTime = date.AddHours(9).AddMinutes(checkInMin);
                    
                    // Check out between 17:00 and 18:00
                    var checkOutMin = rand.Next(0, 60);
                    var checkOutTime = date.AddHours(17).AddMinutes(checkOutMin);

                    logs.Add(new AttendanceLog
                    {
                        UserInfoId = emp.Id,
                        PunchTime = checkInTime,
                        PunchType = PunchType.CheckIn,
                        CreatedAt = checkInTime
                    });

                    logs.Add(new AttendanceLog
                    {
                        UserInfoId = emp.Id,
                        PunchTime = checkOutTime,
                        PunchType = PunchType.CheckOut,
                        CreatedAt = checkOutTime
                    });
                }
            }
        }
        context.AttendanceLogs.AddRange(logs);
        await context.SaveChangesAsync();

        // 8. Seed Leave Requests
        var leaveRequests = new List<LeaveRequest>();
        var leaveTypes = new[] { LeaveType.Sick, LeaveType.Personal, LeaveType.Unpaid, LeaveType.WorkFromHome };
        var statuses = new[] { LeaveStatus.Pending, LeaveStatus.Approved, LeaveStatus.Rejected };

        for (int i = 0; i < 20; i++)
        {
            var emp = allEmployees[rand.Next(allEmployees.Count)];
            var start = DateTime.UtcNow.AddDays(rand.Next(-10, 20));
            var duration = rand.Next(1, 5);

            leaveRequests.Add(new LeaveRequest
            {
                UserInfoId = emp.Id,
                LeaveType = leaveTypes[rand.Next(leaveTypes.Length)],
                StartDate = start,
                EndDate = start.AddDays(duration),
                Status = statuses[rand.Next(statuses.Length)],
                Reason = "Personal reasons",
                CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(1, 10))
            });
        }
        context.LeaveRequests.AddRange(leaveRequests);
        await context.SaveChangesAsync();

        // 9. Seed Bonus Requests
        var bonusRequests = new List<BonusRequest>();
        var bonusStatuses = new[] { BonusStatus.Pending, BonusStatus.Approved, BonusStatus.Rejected };
        for (int i = 0; i < 10; i++)
        {
            var emp = allEmployees[rand.Next(allEmployees.Count)];
            var mgrId = emp.DirectManagerId ?? 1; // Default to admin if no manager
            bonusRequests.Add(new BonusRequest
            {
                RequestingManagerId = mgrId,
                TargetUserId = emp.Id,
                Value = rand.Next(100, 1000),
                Type = BonusType.Amount,
                Reason = "Outstanding performance on recent project",
                Status = bonusStatuses[rand.Next(bonusStatuses.Length)],
                Year = DateTime.UtcNow.Year,
                Month = DateTime.UtcNow.Month,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(1, 10))
            });
        }
        context.BonusRequests.AddRange(bonusRequests);
        await context.SaveChangesAsync();

        // 10. Seed Notifications
        var notifications = new List<Notification>();
        foreach (var req in leaveRequests.Where(r => r.Status == LeaveStatus.Pending))
        {
            var mgrId = context.UserInfos.Where(u => u.Id == req.UserInfoId).Select(u => u.DirectManagerId).FirstOrDefault();
            if (mgrId.HasValue)
            {
                notifications.Add(new Notification
                {
                    UserId = mgrId.Value,
                    Title = "New Leave Request",
                    Message = $"User {req.UserInfoId} requested time off.",
                    Type = NotificationType.RequestPending,
                    IsRead = false,
                    Data = req.Id.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync();
    }
}
