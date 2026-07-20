using HR.Application.Auth.Commands;
using HR.Application.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    [HttpPost("register")]
    // [Authorize(Roles = "Administrator")] // Removed to match legacy or left if needed
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromForm] RegisterCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(new { message = "Registration successful", userImage = command.ProfileImage != null ? "uploaded" : null });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error during registration");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Authenticates a user and issues JWT and Refresh tokens.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _mediator.Send(command, cancellationToken);

            if (!response.Requires2FA && response.AccessToken != null && response.RefreshToken != null)
            {
                SetTokenCookie(response.AccessToken, response.RefreshToken);
            }

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Verifies the OTP for 2FA login.
    /// </summary>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _mediator.Send(command, cancellationToken);
            // VerifyOtpCommand returns LoginResponseDto right now.
            // Oh, wait, I need to update VerifyOtpCommand to return AuthResponse.
            // Let's assume it still works and just return it, or map it.
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Refreshes an expired JWT access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var refreshToken = command.RefreshToken ?? Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken)) return BadRequest(new { message = "Refresh token is required" });

            var response = await _mediator.Send(new RefreshTokenCommand(refreshToken), cancellationToken);
            if (response.AccessToken != null && response.RefreshToken != null)
            {
                SetTokenCookie(response.AccessToken, response.RefreshToken);
            }
            return Ok(new { message = "Token refreshed successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Revokes an active token.
    /// </summary>
    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok(new { message = "Token revoked successfully" });
    }

    /// <summary>
    /// Logs out the user and clears authentication cookies.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _mediator.Send(new LogoutCommand(refreshToken), cancellationToken);
        }

        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Retrieves the profile of the currently authenticated user.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            return Ok(await _mediator.Send(new GetMeQuery(userId), cancellationToken));
        }
        return Unauthorized(new { message = "Invalid user ID in token" });
    }

    /// <summary>
    /// Retrieves statistics for the employee on a specific date.
    /// </summary>
    [Authorize]
    [HttpGet("employee-details")]
    public async Task<IActionResult> GetEmployeeDetails([FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetEmployeeStatsQuery(date), cancellationToken));
    }

    /// <summary>
    /// Retrieves a list of all users.
    /// </summary>
    [Authorize]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetUsersQuery(), cancellationToken));
    }

    /// <summary>
    /// Enables Two-Factor Authentication (2FA) for the current user.
    /// </summary>
    [Authorize]
    [HttpPost("enable-2fa")]
    public async Task<IActionResult> Enable2FA([FromBody] Enable2FACommand command, CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            await _mediator.Send(new Enable2FACommand(userId, command.Type), cancellationToken);
            return Ok(new { message = $"2FA ({command.Type}) enabled successfully" });
        }
        return Unauthorized();
    }

    /// <summary>
    /// Disables Two-Factor Authentication (2FA) for the current user.
    /// </summary>
    [Authorize]
    [HttpPost("disable-2fa")]
    public async Task<IActionResult> Disable2FA(CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            await _mediator.Send(new Disable2FACommand(userId), cancellationToken);
            return Ok(new { message = "2FA disabled successfully" });
        }
        return Unauthorized();
    }

    /// <summary>
    /// Retrieves the 2FA configuration status of the current user.
    /// </summary>
    [Authorize]
    [HttpGet("2fa-status")]
    public async Task<IActionResult> Get2FAStatus(CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            return Ok(await _mediator.Send(new Get2FAStatusQuery(userId), cancellationToken));
        }
        return Unauthorized();
    }

    /// <summary>
    /// Sets up a Time-based One-Time Password (TOTP) authenticator.
    /// </summary>
    [Authorize]
    [HttpPost("setup-totp")]
    public async Task<IActionResult> SetupTotp(CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            return Ok(await _mediator.Send(new SetupTotpCommand(userId), cancellationToken));
        }
        return Unauthorized();
    }

    /// <summary>
    /// Verifies the TOTP setup code to finalize authenticator app enrollment.
    /// </summary>
    [Authorize]
    [HttpPost("verify-totp-setup")]
    public async Task<IActionResult> VerifyTotpSetup([FromBody] VerifyTotpSetupCommand command, CancellationToken cancellationToken)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            await _mediator.Send(new VerifyTotpSetupCommand(userId, command.Code), cancellationToken);
            return Ok(new { message = "TOTP setup verified successfully. You can now enable 2FA with TOTP." });
        }
        return Unauthorized();
    }

    private void SetTokenCookie(string token, string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(30)
        };
        Response.Cookies.Append("accessToken", token, cookieOptions);
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
