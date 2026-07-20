namespace HR.Application.Attendance.DailyAttendanceSummaries.DTOs;

public class DailyEvaluationExportDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string ShiftName { get; set; } = string.Empty;
    public string TimeIn { get; set; } = string.Empty;
    public string TimeOut { get; set; } = string.Empty;
    public int DelayMinutes { get; set; }
    public decimal DeductedDays { get; set; }
    public decimal OvertimeHours { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsDeductionApplied { get; set; }
}
