using HR.Application.Common.Interfaces;
using HR.Application.Auth.Commands;
using HR.Domain.Entities;
using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace HR.Application.Mobile.Commands;

// --- Commands ---
public record MobileLoginCommand(string Email, string Password, string? FcmToken) : IRequest<AuthResponse>;
public record UpdateFcmTokenCommand(int UserId, string FcmToken) : IRequest<bool>;
public record MobileCheckInCommand(int UserId, string? DeviceIp) : IRequest<bool>;
public record MobileCheckOutCommand(int UserId, string? DeviceIp) : IRequest<bool>;
public record ApproveLeaveByHRCommand(int LeaveId, int HRUserId, string? Comment) : IRequest<bool>;
public record RejectLeaveByHRCommand(int LeaveId, int HRUserId, string? Comment) : IRequest<bool>;
public record SendNotificationCommand(int UserId, string Title, string Body) : IRequest<bool>;

// --- Handlers ---
public class MobileCommandsHandler :
    IRequestHandler<MobileLoginCommand, AuthResponse>,
    IRequestHandler<UpdateFcmTokenCommand, bool>,
    IRequestHandler<MobileCheckInCommand, bool>,
    IRequestHandler<MobileCheckOutCommand, bool>,
    IRequestHandler<ApproveLeaveByHRCommand, bool>,
    IRequestHandler<RejectLeaveByHRCommand, bool>,
    IRequestHandler<SendNotificationCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IHttpClientFactory _httpClientFactory;

    public MobileCommandsHandler(IApplicationDbContext context, ITokenService tokenService, IPasswordHasher passwordHasher, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _httpClientFactory = httpClientFactory;
    }

    private void SendFcmToWebhook(string email, string fcmToken)
    {
        // Fire and forget
        Task.Run(async () =>
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var payload = new { email = email, fcm_token = fcmToken };
                await client.PostAsJsonAsync("https://twokeyok-n.mustafa-esam.dev/subscribe-twokeyok/", payload);
            }
            catch
            {
                // Ignore webhook errors
            }
        });
    }

    public async Task<AuthResponse> Handle(MobileLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos
            .Include(u => u.Department)
            .Include(u => u.Section)
            .Include(u => u.AttendanceShift)
            .Include(u => u.SecondLineManager)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        // Mobile specific: Update FCM
        if (!string.IsNullOrEmpty(request.FcmToken))
        {
            user.FcmToken = request.FcmToken;
            SendFcmToWebhook(user.Email, request.FcmToken);
        }

        if (user.Is2FAEnabled)
        {
            user.CurrentOtp = new Random().Next(100000, 999999).ToString();
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(5);
            await _context.SaveChangesAsync(cancellationToken);
            return new AuthResponse(null, null, null, true, user.Id, user.TwoFactorType);
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
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
            MaritalStatus = user.MaritalStatus,
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

    public async Task<bool> Handle(UpdateFcmTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null) return false;

        user.FcmToken = request.FcmToken;
        await _context.SaveChangesAsync(cancellationToken);

        SendFcmToWebhook(user.Email, request.FcmToken);

        return true;
    }

    public async Task<bool> Handle(MobileCheckInCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null) throw new KeyNotFoundException("User not found");

        var log = new AttendanceLog
        {
            UserInfoId = user.Id,
            PunchTime = DateTime.UtcNow,
            LogsType = LogType.Mobile,
            PunchType = PunchType.CheckIn
        };

        // If you had latitude/longitude columns in AttendanceLog, you'd set them here.

        _context.AttendanceLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(MobileCheckOutCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null) throw new KeyNotFoundException("User not found");

        var log = new AttendanceLog
        {
            UserInfoId = user.Id,
            PunchTime = DateTime.UtcNow,
            LogsType = LogType.Mobile,
            PunchType = PunchType.CheckOut
        };

        _context.AttendanceLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(ApproveLeaveByHRCommand request, CancellationToken cancellationToken)
    {
        var leave = await _context.LeaveRequests.FindAsync(new object[] { request.LeaveId }, cancellationToken);
        if (leave == null) throw new KeyNotFoundException("Leave request not found");

        leave.Status = LeaveStatus.Approved;
        // Optionally store who approved it: leave.ApprovedById = request.HRUserId;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(RejectLeaveByHRCommand request, CancellationToken cancellationToken)
    {
        var leave = await _context.LeaveRequests.FindAsync(new object[] { request.LeaveId }, cancellationToken);
        if (leave == null) throw new KeyNotFoundException("Leave request not found");

        leave.Status = LeaveStatus.Rejected;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null || string.IsNullOrEmpty(user.FcmToken)) return false;

        // Save notification to DB
        var notification = new Notification
        {
            UserId = user.Id,
            Title = request.Title,
            Message = request.Body,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Call IFirebaseService if injected to actually send the push notification
        return true;
    }
}
