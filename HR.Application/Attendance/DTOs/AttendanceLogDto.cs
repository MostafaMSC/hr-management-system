using HR.Domain.Enums;

namespace HR.Application.Attendance.DTOs;

public class AttendanceLogDto
{
    public string UserID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public string? Role { get; set; }
    public string? DeviceIP { get; set; }
    public string? CheckStatus { get; set; }
    public LogType LogsType { get; set; } = LogType.FingerPrint;
}
