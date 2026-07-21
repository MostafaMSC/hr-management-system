using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Domain.Entities;
using HR.Application.Attendance.ZKPython.WorkHours.DTOs;
using HR.Application.Common.Interfaces;

namespace HR.Application.Attendance.ZKPython.WorkHours.Queries;

public class GetWorkHoursQueryHandler : IRequestHandler<GetWorkHoursQuery, List<WorkHoursDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IAttendanceLogRepository _logRepository;

    public GetWorkHoursQueryHandler(IUserRepository userRepository, IAttendanceLogRepository logRepository)
    {
        _userRepository = userRepository;
        _logRepository = logRepository;
    }

    public async Task<List<WorkHoursDto>> Handle(GetWorkHoursQuery request, CancellationToken cancellationToken)
    {
        if (!TimeSpan.TryParse(request.Time, out TimeSpan startTime))
            startTime = new TimeSpan(8, 30, 0);

        if (!TimeSpan.TryParse(request.FinishTime, out TimeSpan finishTimeSpan))
            finishTimeSpan = new TimeSpan(16, 0, 0);

        var today = DateTime.UtcNow.Date;
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Fix: Week starts on Saturday (DayOfWeek 6) in MENA
        int daysSinceSaturday = ((int)today.DayOfWeek - (int)DayOfWeek.Saturday + 7) % 7;
        var startOfWeek = today.AddDays(-daysSinceSaturday);
        var endOfWeek = startOfWeek.AddDays(6);

        double monthlyRequired = request.RequiredDailyHours * request.WorkingDaysPerMonth;

        // Base: All Users from IUserRepository
        var allUsers = await _userRepository.GetAllUsersAsync(cancellationToken);

        // Logs: All logs for this month from IAttendanceLogRepository
        // Note: GetLogsForPeriodAsync takes DateTime range.
        var allLogs = await _logRepository.GetLogsForPeriodAsync(firstDayOfMonth, today.AddDays(1).AddSeconds(-1), request.DeviceIp);
        // Note: Added AddDays(1) to include today fully if needed, or if repo handles "inclusive".
        // Assuming Repo uses <= to.

        // Helper function (moved from local function to private method if possible, or kept local)
        // Keeping it local to Capture closures effectively or simplified logic
        double CalculateEnhancedHours(IEnumerable<AttendanceLog> logs, bool isIncludeLiveToday = false)
        {
            var dailyGroups = logs.GroupBy(x => x.PunchTime.Date);
            double totalHours = 0;
            var todayLocal = DateTime.UtcNow.Date;

            foreach (var dayGroup in dailyGroups)
            {
                var date = dayGroup.Key;
                var dayLogs = dayGroup.OrderBy(x => x.PunchTime).ToList();
                if (!dayLogs.Any()) continue;

                var checkIn = dayLogs.FirstOrDefault(x => IsCheckIn(x.CheckStatus));
                var checkOut = dayLogs.LastOrDefault(x => IsCheckOut(x.CheckStatus));

                // If it's today and we want live calculation up to 'Now' (and not yet checked out)
                if (date == todayLocal && isIncludeLiveToday)
                {
                    var punchIn = checkIn?.Time ?? dayLogs.First().Time;
                    if (checkOut != null && (checkOut.Time - punchIn).TotalMinutes > 30)
                    {
                        // They have a checkout, just use the recorded duration
                        totalHours += (checkOut.Time - punchIn).TotalHours;
                    }
                    else if (checkOut == null || (checkOut.Time - punchIn).TotalMinutes <= 30)
                    {
                        // No valid checkout yet, or just recently checked out (fallback to live)
                        var finishDateTime = todayLocal.Add(finishTimeSpan);
                        var now = DateTime.UtcNow;
                        var endTime = now > finishDateTime ? finishDateTime : now;

                        if (endTime > punchIn)
                        {
                            totalHours += (endTime - punchIn).TotalHours;
                        }
                    }
                }
                else
                {
                    // Historical day or today without live calc
                    if (checkIn != null && checkOut != null && checkIn != checkOut)
                    {
                        totalHours += (checkOut.Time - checkIn.Time).TotalHours;
                    }
                    else
                    {
                        // Fallback: Max - Min if duration > 30 mins
                        var first = dayLogs.First().Time;
                        var last = dayLogs.Last().Time;
                        if ((last - first).TotalMinutes > 30)
                        {
                            totalHours += (last - first).TotalHours;
                        }
                    }
                }
            }
            return totalHours;
        }

        var result = allUsers.Select(user =>
        {
            var userLogs = allLogs.Where(l => l.UserInfoId == user.Id).ToList();

            // Calculate 'TodayHours' separately (always live)
            var todayLogs = userLogs.Where(x => x.PunchTime.Date == today).ToList();
            double todayHours = CalculateEnhancedHours(todayLogs, isIncludeLiveToday: true);

            // Weekly and Monthly: include live today so the totals are accurate
            var weekLogs = userLogs.Where(x => x.PunchTime.Date >= startOfWeek && x.PunchTime.Date <= endOfWeek).ToList();
            var weeklyHours = CalculateEnhancedHours(weekLogs, isIncludeLiveToday: true);

            var monthlyHours = CalculateEnhancedHours(userLogs, isIncludeLiveToday: true);

            double achievementPercent = monthlyRequired > 0 ? (monthlyHours / monthlyRequired) * 100 : 0;
            double deductionPercent = monthlyRequired > 0 ? Math.Max(0, ((monthlyRequired - monthlyHours) / monthlyRequired) * 100) : 0;

            return new WorkHoursDto
            {
                UserID = user.BiometricId ?? user.Username,
                Name = user.Username,
                TodayHours = Math.Round(todayHours, 2),
                WeeklyHours = Math.Round(weeklyHours, 2),
                MonthHours = Math.Round(monthlyHours, 2),
                MonthlyRequired = Math.Round(monthlyRequired, 2),
                AchievementPercent = Math.Round(achievementPercent, 2),
                DeductionPercent = Math.Round(deductionPercent, 2)
            };
        }).ToList();

        return result;
    }

    private bool IsCheckIn(string? status) => status == "0" || status == "CheckIn" || status == "Check In";
    private bool IsCheckOut(string? status) => status == "1" || status == "CheckOut" || status == "Check Out";
}
