using HR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace HR.Application.Auth.Commands;

// --- Commands ---
public record RevokeTokenCommand(string RefreshToken) : IRequest<bool>;
public record LogoutCommand(string RefreshToken) : IRequest<bool>;
public record UpdateProfilePictureCommand(int UserId, IFormFile ProfilePicture) : IRequest<string>;

// --- Handlers ---
public class AuthProfileCommandsHandler :
    IRequestHandler<RevokeTokenCommand, bool>,
    IRequestHandler<LogoutCommand, bool>,
    IRequestHandler<UpdateProfilePictureCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ILocalStorageService _localStorage;

    public AuthProfileCommandsHandler(IApplicationDbContext context, ILocalStorageService localStorage)
    {
        _context = context;
        _localStorage = localStorage;
    }

    public async Task<bool> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == request.RefreshToken, cancellationToken);
        if (token != null)
        {
            token.IsRevoked = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
        return true;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // Logout typically revokes the refresh token and optionally clears FCM
        return await Handle(new RevokeTokenCommand(request.RefreshToken), cancellationToken);
    }

    public async Task<string> Handle(UpdateProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null) throw new KeyNotFoundException("User not found.");

        if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
        {
            _localStorage.DeleteFile(user.ProfilePictureUrl);
        }

        var newUrl = await _localStorage.SaveFileAsync(request.ProfilePicture, "avatars");
        user.ProfilePictureUrl = newUrl;

        await _context.SaveChangesAsync(cancellationToken);
        return newUrl;
    }
}
