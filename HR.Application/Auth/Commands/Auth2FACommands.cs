using HR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HR.Application.Auth.Commands;

namespace HR.Application.Auth.Commands;

// --- Commands ---
public record VerifyOtpCommand(string Email, string Otp) : IRequest<AuthResponse>;
public record SendPasswordResetOtpCommand(string Email) : IRequest<bool>;
public record ResetPasswordWithOtpCommand(string Email, string Otp, string NewPassword) : IRequest<bool>;
public record Enable2FACommand(int UserId, string Type) : IRequest<bool>;
public record Disable2FACommand(int UserId) : IRequest<bool>;
public record SetupTotpCommand(int UserId) : IRequest<SetupTotpResponse>;
public record VerifyTotpSetupCommand(int UserId, string Code) : IRequest<bool>;

// --- Handlers ---
public class Auth2FACommandsHandler :
    IRequestHandler<VerifyOtpCommand, AuthResponse>,
    IRequestHandler<SendPasswordResetOtpCommand, bool>,
    IRequestHandler<ResetPasswordWithOtpCommand, bool>,
    IRequestHandler<Enable2FACommand, bool>,
    IRequestHandler<Disable2FACommand, bool>,
    IRequestHandler<SetupTotpCommand, SetupTotpResponse>,
    IRequestHandler<VerifyTotpSetupCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public Auth2FACommandsHandler(IApplicationDbContext context, ITokenService tokenService, IPasswordHasher passwordHasher)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos
            .Include(u => u.Department)
            .Include(u => u.Section)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null || user.CurrentOtp != request.Otp || user.OtpExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired OTP.");

        // Clear OTP
        user.CurrentOtp = null;
        user.OtpExpiry = null;

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto
        {
            Id = user.Id,

            Email = user.Email,
            Role = user.Role,
            DepartmentId = user.DepartmentId,
            DepartmentName = user.Department?.Name,
            SectionId = user.SectionId,
            SectionName = user.Section?.Name,
            Photo = user.ProfilePictureUrl
        };

        return new AuthResponse(accessToken, refreshToken.Token, refreshToken.ExpiryDate, false, user.Id, null, userDto);
    }

    public async Task<bool> Handle(SendPasswordResetOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user != null)
        {
            user.CurrentOtp = new Random().Next(100000, 999999).ToString();
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(15);
            await _context.SaveChangesAsync(cancellationToken);

            // TODO: Send via EmailService
        }
        // Always return true to prevent email enumeration
        return true;
    }

    public async Task<bool> Handle(ResetPasswordWithOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null || user.CurrentOtp != request.Otp || user.OtpExpiry < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired OTP.");

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.CurrentOtp = null;
        user.OtpExpiry = null;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(Enable2FACommand request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null) throw new KeyNotFoundException("User not found.");

        user.Is2FAEnabled = true;
        user.TwoFactorType = request.Type;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(Disable2FACommand request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null) throw new KeyNotFoundException("User not found.");

        user.Is2FAEnabled = false;
        user.TwoFactorType = null;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<SetupTotpResponse> Handle(SetupTotpCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null) throw new KeyNotFoundException("User not found.");

        var secret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
        user.AuthenticatorSecretKey = secret;
        await _context.SaveChangesAsync(cancellationToken);

        return new SetupTotpResponse
        {
            Secret = secret,
            QrCodeUri = $"otpauth://totp/HRManagement:{user.Email}?secret={secret}&issuer=HRManagement"
        };
    }

    public async Task<bool> Handle(VerifyTotpSetupCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null) throw new KeyNotFoundException("User not found.");

        // Simplified verification for now (just checking if it's not empty, real implementation needs a TOTP library)
        if (string.IsNullOrEmpty(request.Code)) throw new UnauthorizedAccessException("Invalid code.");

        return true;
    }
}
