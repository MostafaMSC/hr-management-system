using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Reports.Queries;

public record GetWeeklyLateQuery(string Time, string? DeviceIp) : IRequest<GetWeeklyLateResult>;

public class GetWeeklyLateResult 
{
    public string WeekStart { get; set; } = string.Empty;
    public string WeekEnd { get; set; } = string.Empty;
    public string RequiredTime { get; set; } = string.Empty;
    public List<WeeklyLateReportDto> Result { get; set; } = new();
}

public class GetWeeklyLateQueryHandler : IRequestHandler<GetWeeklyLateQuery, GetWeeklyLateResult>
{
    private readonly IAttendanceLogRepository _repository;

    public GetWeeklyLateQueryHandler(IAttendanceLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetWeeklyLateResult> Handle(GetWeeklyLateQuery request, CancellationToken cancellationToken)
    {
        if (!TimeSpan.TryParse(request.Time, out TimeSpan lateTime))
            lateTime = new TimeSpan(8, 30, 0);

        var today = DateTime.Today;
        int daysSinceSaturday = ((int)today.DayOfWeek + 1) % 7;
        var startOfWeek = today.AddDays(-daysSinceSaturday);

        var weeklyLogs = await _repository.GetLogsForPeriodAsync(startOfWeek, today, request.DeviceIp);

        var result = weeklyLogs
            .GroupBy(l => new { l.UserID, l.Name })
            .Select(g => {
                var dailyFirstEntry = g
                    .GroupBy(x => x.PunchTime.Date)
                    .Select(dayGroup => new {
                        Date = dayGroup.Key,
                        DayName = GetArabicDayName(dayGroup.Key.DayOfWeek),
                        FirstEntry = dayGroup.Min(x => x.PunchTime),
                        FirstEntryTime = dayGroup.Min(x => x.PunchTime).TimeOfDay
                    })
                    .ToList();

                var dailyLate = dailyFirstEntry
                    .Where(d => d.FirstEntryTime > lateTime)
                    .Select(d => new DailyLateDetailDto {
                        Date = d.Date,
                        DayName = d.DayName,
                        EntryTime = d.FirstEntry.ToString("HH:mm:ss"),
                        LateMinutes = Math.Round((d.FirstEntryTime - lateTime).TotalMinutes, 0)
                    })
                    .ToList();

                var totalLateMinutes = dailyLate.Sum(d => d.LateMinutes);
                
                return new WeeklyLateReportDto {
                    UserID = g.Key.UserID.ToString(),
                    Name = g.Key.Name,
                    LateDaysCount = dailyLate.Count,
                    TotalLateMinutes = totalLateMinutes,
                    TotalLateHours = Math.Round(totalLateMinutes / 60.0, 2),
                    DailyDetails = dailyLate
                };
            })
            .Where(x => x.LateDaysCount > 0)
            .OrderByDescending(x => x.TotalLateMinutes)
            .ToList();

         return new GetWeeklyLateResult
         {
             WeekStart = startOfWeek.ToString("yyyy-MM-dd"),
             WeekEnd = today.ToString("yyyy-MM-dd"),
             RequiredTime = request.Time,
             Result = result
         };
    }

    private static string GetArabicDayName(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Saturday => "Ø§Ù„Ø³Ø¨Øª",
            DayOfWeek.Sunday => "Ø§Ù„Ø£Ø­Ø¯",
            DayOfWeek.Monday => "Ø§Ù„Ø§Ø«Ù†ÙŠÙ†",
            DayOfWeek.Tuesday => "Ø§Ù„Ø«Ù„Ø§Ø«Ø§Ø¡",
            DayOfWeek.Wednesday => "Ø§Ù„Ø£Ø±Ø¨Ø¹Ø§Ø¡",
            DayOfWeek.Thursday => "Ø§Ù„Ø®Ù…ÙŠØ³",
            DayOfWeek.Friday => "Ø§Ù„Ø¬Ù…Ø¹Ø©",
            _ => ""
        };
    }
}
