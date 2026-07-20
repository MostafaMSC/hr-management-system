using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
namespace HR.Application.Attendance.ZKPython.Users.DTOs;

public class EmployeeStatsDto
{
    public string Username { get; set; } = string.Empty;
    public DateTime ReferenceDate { get; set; }
    public StatsBucket Today { get; set; } = new();
    public StatsBucket Weekly { get; set; } = new();
    public StatsBucket Monthly { get; set; } = new();
    public List<AttendanceLogDto> Logs { get; set; } = new();
    public List<DailySummaryDto> DailySummaries { get; set; } = new();
}

public class AttendanceLogDto
{
    public DateTime Time { get; set; }
    public string? CheckStatus { get; set; }
    public string? DeviceIP { get; set; }
}

public class DailySummaryDto
{
    public DateTime Date { get; set; }
    public double WorkMinutes { get; set; }
    public double LateMinutes { get; set; }
    public bool IsPresent { get; set; }
}

public class StatsBucket
{
    public double WorkMinutes { get; set; }
    public double LateMinutes { get; set; }
    public int LateDaysCount { get; set; }
    public double PersonalLeaveHours { get; set; }
    public double SickLeaveHours { get; set; }
}
