namespace HR.Application.Attendance.DTOs;

public class DeviceUserResultDto
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Privilege { get; set; }
    public string Password { get; set; } = string.Empty;
    public string Card { get; set; } = string.Empty;
    public List<DeviceFingerprintDto> Fingerprints { get; set; } = new();
}
