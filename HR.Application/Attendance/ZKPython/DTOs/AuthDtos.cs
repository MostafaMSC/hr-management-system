using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Domain.Enums;
using HR.Domain.Entities;
using Microsoft.AspNetCore.Http;
using HR.Domain.ValueObjects;
namespace HR.Application.Attendance.ZKPython.DTOs;

public record LoginRequest(string Username, string Password);

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? DeviceIp { get; set; }
    public int? DepartmentId { get; set; }
    public int? SectionId { get; set; }
    public PhoneNumber? PhoneNumber { get; set; }
    public string? Card { get; set; }
    public string? Address { get; set; }
    public UserType? Role { get; set; } = HR.Domain.Enums.UserType.Employee;
    public Gender? Gender { get; set; }
    public ShiftType? ShiftType { get; set; }
    public AccountStatus? AccountStatus { get; set; } = HR.Domain.Enums.AccountStatus.Active;
    public DateTime? BirthDate { get; set; }
    public DateTime? HireDate { get; set; }
    public bool Is2FAEnabled { get; set; }
    public IFormFile? ProfileImage { get; set; }
}

public record AuthResponse(
    string? AccessToken,
    string? RefreshToken,
    DateTime? ExpiresAt,
    bool Requires2FA = false,
    int? UserId = null,
    string? TwoFactorType = null,
    UserDto? User = null
);

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public HR.Domain.Enums.UserType Role { get; set; } = HR.Domain.Enums.UserType.Employee;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? SectionId { get; set; }
    public string? SectionName { get; set; }
    public string? Photo { get; set; }
}

public record RefreshTokenRequest(string RefreshToken);

public record TokenValidationResult(
    bool IsValid,
    string? Error = null,
    int? UserId = null
);

public class VerifyOtpRequest
{
    public int UserId { get; set; }
    public string Otp { get; set; } = string.Empty;
}

public class Enable2FARequest
{
    public TwoFactorType Type { get; set; } = HR.Domain.Enums.TwoFactorType.Email;
}

public class SetupTotpResponse
{
    public string Secret { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
}

public class VerifyTotpSetupRequest
{
    public string Code { get; set; } = string.Empty;
}

public class Debug2FaResponse
{
    public bool Is2FAEnabled { get; set; }
    public string? TwoFactorType { get; set; }
    public string? TwoFactorSecret { get; set; }
    public string? TwoFactorCodeHash { get; set; }
    public DateTime? TwoFactorExpiry { get; set; }
    public int TwoFactorFailedAttempts { get; set; }
    public string? CurrentCode { get; set; }
    public int RemainingSeconds { get; set; }
}
