namespace HR.Application.Attendance.DTOs;

public class DeviceFingerprintDto
{
    public int FingerIndex { get; set; }
    public string Template { get; set; } = string.Empty; // Base64 string
}
