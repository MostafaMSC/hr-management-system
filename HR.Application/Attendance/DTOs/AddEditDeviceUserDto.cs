namespace HR.Application.Attendance.DTOs;

public class AddEditDeviceUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Password { get; set; } = string.Empty;
    public string? Card { get; set; } = "0";
    public int? Privilege { get; set; } = 0;
}
