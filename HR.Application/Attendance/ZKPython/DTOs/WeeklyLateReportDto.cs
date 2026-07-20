using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
namespace HR.Application.Attendance.ZKPython.DTOs;

public class WeeklyLateReportDto
{
    public string UserID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int LateDaysCount { get; set; }
    public double TotalLateMinutes { get; set; }
    public double TotalLateHours { get; set; }
    public List<DailyLateDetailDto> DailyDetails { get; set; } = new();
}

public class DailyLateDetailDto
{
    public DateTime Date { get; set; }
    public string DayName { get; set; } = string.Empty;
    public string EntryTime { get; set; } = string.Empty;
    public double LateMinutes { get; set; }
}
