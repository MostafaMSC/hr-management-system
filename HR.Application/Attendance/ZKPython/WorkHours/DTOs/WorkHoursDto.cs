using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
namespace HR.Application.Attendance.ZKPython.WorkHours.DTOs;

public class WorkHoursDto
{
    public string UserID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double TodayHours { get; set; }
    public double WeeklyHours { get; set; }
    public double MonthHours { get; set; }
    public double MonthlyRequired { get; set; }
    public double AchievementPercent { get; set; }
    public double DeductionPercent { get; set; }
}
