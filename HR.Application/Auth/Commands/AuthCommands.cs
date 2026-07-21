using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Http;

namespace HR.Application.Auth.Commands;

// --- DTOs ---
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
    public string? PhoneNumber { get; set; }
    public DateTime? BirthDate { get; set; }
    public DateTime? HireDate { get; set; }
    public int ActualAlreadyTakenLeaves { get; set; }
    public string? Gender { get; set; }
    public string? ShiftType { get; set; }
    public int? AttendanceShiftId { get; set; }
    public string? AttendanceShiftName { get; set; }
    public string? Address { get; set; }
    public string? Card { get; set; }
    public string? AccountStatus { get; set; }
    public string? EmployeeId { get; set; }
    public string? FcmToken { get; set; }
    public string? FullNameArabic { get; set; }
    public string? MotherName { get; set; }
    public string? PersonalEmail { get; set; }
    public string? PersonalPhoneNumber { get; set; }
    public string? JobTitle { get; set; }
    public int? DirectManagerId { get; set; }
    public int? ReportToId { get; set; }
    public int? SecondLineManagerId { get; set; }
    public string? SecondLineManagerName { get; set; }
    public string? MaritalStatus { get; set; }
    public string? City { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? EducationalLevel { get; set; }
    public string? FieldOfStudy { get; set; }
    public string? WorkLocation { get; set; }
    public string? Nationality { get; set; }
    public int OvertimeBalanceHours { get; set; }
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

public class SetupTotpResponse
{
    public string Secret { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
}

// --- Commands ---
public record LoginCommand(string Username, string Password, double? ExpiryMinutes = null) : IRequest<AuthResponse>;

public class RegisterCommand : IRequest<bool>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? DeviceIp { get; set; }
    public int? DepartmentId { get; set; }
    public int? SectionId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Card { get; set; }
    public string? Address { get; set; }
    public HR.Domain.Enums.UserType? Role { get; set; } = HR.Domain.Enums.UserType.Employee;
    public string? Gender { get; set; }
    public string? ShiftType { get; set; }
    public string? AccountStatus { get; set; } = "Active";
    public DateTime? BirthDate { get; set; }
    public DateTime? HireDate { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public IFormFile? ProfileImage { get; set; }
}

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;

// --- Handlers ---
public class AuthCommandsHandler :
    IRequestHandler<LoginCommand, AuthResponse>,
    IRequestHandler<RegisterCommand, bool>,
    IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthCommandsHandler(IApplicationDbContext context, ITokenService tokenService, IPasswordHasher passwordHasher)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos
            .Include(u => u.Department)
            .Include(u => u.Section)
            .Include(u => u.AttendanceShift)
            .Include(u => u.SecondLineManager)
            .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Username, cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        // Check 2FA
        if (user.Is2FAEnabled)
        {
            user.CurrentOtp = new Random().Next(100000, 999999).ToString();
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(5);
            await _context.SaveChangesAsync(cancellationToken);
            return new AuthResponse(null, null, null, true, user.Id, user.TwoFactorType);
        }

        var accessToken = _tokenService.GenerateAccessToken(user, request.ExpiryMinutes);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            DepartmentId = user.DepartmentId,
            DepartmentName = user.Department?.Name,
            SectionId = user.SectionId,
            SectionName = user.Section?.Name,
            Photo = user.ProfilePictureUrl,
            PhoneNumber = user.PhoneNumber,
            BirthDate = user.BirthDate,
            HireDate = user.HireDate,
            ActualAlreadyTakenLeaves = user.ActualAlreadyTakenLeaves,
            Gender = user.Gender,
            ShiftType = user.ShiftType,
            AttendanceShiftId = user.AttendanceShiftId,
            AttendanceShiftName = user.AttendanceShift?.Name,
            Address = user.Address,
            Card = user.Card,
            AccountStatus = user.AccountStatus,
            EmployeeId = user.BiometricId,
            FcmToken = user.FcmToken,
            FullNameArabic = user.FullNameArabic,
            MotherName = user.MotherName,
            PersonalEmail = user.PersonalEmail,
            PersonalPhoneNumber = user.PersonalPhoneNumber,
            JobTitle = user.JobTitle,
            DirectManagerId = user.DirectManagerId,
            ReportToId = user.ReportToId,
            SecondLineManagerId = user.SecondLineManagerId,
            SecondLineManagerName = user.SecondLineManager != null ? $"{user.SecondLineManager.FirstName} {user.SecondLineManager.LastName}" : null,
            MaritalStatus = user.MaritalStatus?.ToString(),
            City = user.City,
            EmergencyContactPhone = user.EmergencyContactPhone,
            EmergencyContactRelation = user.EmergencyContactRelation,
            EducationalLevel = user.EducationalLevel,
            FieldOfStudy = user.FieldOfStudy,
            WorkLocation = user.WorkLocation,
            Nationality = user.Nationality,
            OvertimeBalanceHours = user.OvertimeBalanceHours
        };

        return new AuthResponse(accessToken, refreshToken.Token, refreshToken.ExpiryDate, false, user.Id, null, userDto);
    }

    public async Task<bool> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _context.UserInfos.AnyAsync(u => u.Username == request.Username, cancellationToken))
            throw new InvalidOperationException("Username or Email already exists.");

        string? imagePath = null;
        if (request.ProfileImage != null && request.ProfileImage.Length > 0)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "users");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(request.ProfileImage.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.ProfileImage.CopyToAsync(stream, cancellationToken);
            }
            imagePath = Path.Combine("uploads", "users", fileName).Replace("\\", "/");
        }

        var user = new UserInfo
        {
            Username = request.Username,
            FirstName = request.Username, // Mapping
            LastName = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = request.Role ?? HR.Domain.Enums.UserType.Employee,
            DepartmentId = request.DepartmentId > 0 ? request.DepartmentId : null,
            SectionId = request.SectionId > 0 ? request.SectionId : null,
            DeviceIp = request.DeviceIp,
            PhoneNumber = request.PhoneNumber,
            Card = request.Card,
            Address = request.Address,
            Gender = request.Gender,
            ShiftType = request.ShiftType,
            AccountStatus = request.AccountStatus ?? "Active",
            BirthDate = request.BirthDate.HasValue ? DateTime.SpecifyKind(request.BirthDate.Value, DateTimeKind.Utc) : null,
            HireDate = request.HireDate.HasValue ? DateTime.SpecifyKind(request.HireDate.Value, DateTimeKind.Utc) : null,
            Is2FAEnabled = request.TwoFactorEnabled,
            DateOfJoining = request.HireDate.HasValue ? DateTime.SpecifyKind(request.HireDate.Value, DateTimeKind.Utc) : DateTime.UtcNow,
            ProfilePictureUrl = imagePath
        };

        _context.UserInfos.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _context.RefreshTokens
            .Include(r => r.UserInfo)
            .ThenInclude(u => u.Department)
            .Include(r => r.UserInfo)
            .ThenInclude(u => u.Section)
            .Include(r => r.UserInfo)
            .ThenInclude(u => u.AttendanceShift)
            .Include(r => r.UserInfo)
            .ThenInclude(u => u.SecondLineManager)
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken, cancellationToken);

        if (storedToken == null || storedToken.IsRevoked || storedToken.IsUsed || storedToken.ExpiryDate < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        storedToken.IsUsed = true;

        var accessToken = _tokenService.GenerateAccessToken(storedToken.UserInfo!);
        var newRefreshToken = _tokenService.GenerateRefreshToken(storedToken.UserInfoId);

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var user = storedToken.UserInfo!;
        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            DepartmentId = user.DepartmentId,
            DepartmentName = user.Department?.Name,
            SectionId = user.SectionId,
            SectionName = user.Section?.Name,
            Photo = user.ProfilePictureUrl,
            PhoneNumber = user.PhoneNumber,
            BirthDate = user.BirthDate,
            HireDate = user.HireDate,
            ActualAlreadyTakenLeaves = user.ActualAlreadyTakenLeaves,
            Gender = user.Gender,
            ShiftType = user.ShiftType,
            AttendanceShiftId = user.AttendanceShiftId,
            AttendanceShiftName = user.AttendanceShift?.Name,
            Address = user.Address,
            Card = user.Card,
            AccountStatus = user.AccountStatus,
            EmployeeId = user.BiometricId,
            FcmToken = user.FcmToken,
            FullNameArabic = user.FullNameArabic,
            MotherName = user.MotherName,
            PersonalEmail = user.PersonalEmail,
            PersonalPhoneNumber = user.PersonalPhoneNumber,
            JobTitle = user.JobTitle,
            DirectManagerId = user.DirectManagerId,
            ReportToId = user.ReportToId,
            SecondLineManagerId = user.SecondLineManagerId,
            SecondLineManagerName = user.SecondLineManager != null ? $"{user.SecondLineManager.FirstName} {user.SecondLineManager.LastName}" : null,
            MaritalStatus = user.MaritalStatus?.ToString(),
            City = user.City,
            EmergencyContactPhone = user.EmergencyContactPhone,
            EmergencyContactRelation = user.EmergencyContactRelation,
            EducationalLevel = user.EducationalLevel,
            FieldOfStudy = user.FieldOfStudy,
            WorkLocation = user.WorkLocation,
            Nationality = user.Nationality,
            OvertimeBalanceHours = user.OvertimeBalanceHours
        };

        return new AuthResponse(accessToken, newRefreshToken.Token, newRefreshToken.ExpiryDate, false, user.Id, null, userDto);
    }
}
