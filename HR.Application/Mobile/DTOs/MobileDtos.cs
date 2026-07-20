namespace HR.Application.Mobile.DTOs;

public record MobileLoginRequest(string Email, string Password, string? FcmToken = null);
public record UpdateFcmTokenRequest(string FcmToken);
public record MobileAttendanceRequest(string? IpAddress);
public record ColleagueDto(int UserId, string UserName);
public record ApproveRejectLeaveDto(string? Comment);
public record VerifyOtpRequest(int UserId, string Otp);
public record RefreshTokenRequest(string? RefreshToken);

public class DepartmentUserSummaryDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPresentToday { get; set; }
}

public class DailyAttendanceLogDto
{
    public DateOnly Date { get; set; }
    public string? CheckIn { get; set; }
    public string? CheckOut { get; set; }
    public string? TotalHours { get; set; }
    public bool HasLeaveRequest { get; set; }
    public string? LeaveType { get; set; }
    public string? LeaveStatus { get; set; }
    public string? LeaveReason { get; set; }
    public string? LeaveFromTime { get; set; }
    public string? LeaveToTime { get; set; }
    public double? LeaveHours { get; set; }
    public string? HolidayName { get; set; }
    public string? Status { get; set; }
}
