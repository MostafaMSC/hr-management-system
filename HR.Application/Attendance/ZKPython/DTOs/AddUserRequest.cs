using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Domain.ValueObjects;
namespace HR.Application.Attendance.ZKPython.DTOs;

public class AddUserRequest
{
    public string DeviceIp { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Department { get; set; }
    public string? Section { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Card { get; set; }
    public string? Address { get; set; }
    public UserType? Role { get; set; }
    public Gender? Gender { get; set; }
    public ShiftType? ShiftType { get; set; }
    public AccountStatus? AccountStatus { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? HireDate { get; set; }
    public bool Is2FAEnabled { get; set; }
}
