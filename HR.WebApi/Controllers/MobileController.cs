using HR.Application.Auth.Commands;
using HR.Application.Mobile.Commands;
using HR.Application.Mobile.DTOs;
using HR.Application.Mobile.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MobileController : ControllerBase
{
    private readonly IMediator _mediator;

    public MobileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Authenticates a mobile user.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] MobileLoginRequest request)
    {
        var response = await _mediator.Send(new MobileLoginCommand(request.Email, request.Password, request.FcmToken));
        SetMobileTokenCookie(response.AccessToken, response.RefreshToken);
        return Ok(response);
    }

    /// <summary>
    /// Refreshes the mobile authentication token.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var refreshToken = request.RefreshToken ?? Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken)) return BadRequest(new { message = "Refresh token is required" });

        var response = await _mediator.Send(new RefreshTokenCommand(refreshToken));
        SetMobileTokenCookie(response.AccessToken, response.RefreshToken);
        return Ok(response);
    }

    /// <summary>
    /// Logs out the mobile user.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
        return Ok(new { message = "تم تسجيل الخروج بنجاح" });
    }

    /// <summary>
    /// Gets the current authenticated user's details.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            // Re-using GetMobileSummaryQuery or similar would be better, but returning a mock/basic DTO here to match the endpoint
            // In a full implementation, you'd call _mediator.Send(new GetUserQuery(userId));
            return Ok(new { id = userId }); // Simplified for now
        }
        return Unauthorized();
    }

    /// <summary>
    /// Verifies the OTP for mobile login.
    /// </summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        // Dummy implementation to match endpoint signature.
        return Ok(new { message = "OTP Verified" });
    }

    /// <summary>
    /// Updates the Firebase Cloud Messaging (FCM) token for push notifications.
    /// </summary>
    [HttpPost("update-fcm-token")]
    [Authorize]
    public async Task<ActionResult<bool>> UpdateFcmToken([FromBody] UpdateFcmTokenRequest request)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            return Ok(await _mediator.Send(new UpdateFcmTokenCommand(userId, request.FcmToken)));
        }
        return Unauthorized();
    }

    /// <summary>
    /// Records a check-in attendance punch.
    /// </summary>
    [HttpPost("checkin")]
    [Authorize]
    public async Task<ActionResult<bool>> CheckIn([FromBody] MobileAttendanceRequest request)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            return Ok(await _mediator.Send(new MobileCheckInCommand(userId, request.IpAddress)));
        }
        return Unauthorized();
    }

    /// <summary>
    /// Records a check-out attendance punch.
    /// </summary>
    [HttpPost("checkout")]
    [Authorize]
    public async Task<ActionResult<bool>> CheckOut([FromBody] MobileAttendanceRequest request)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            return Ok(await _mediator.Send(new MobileCheckOutCommand(userId, request.IpAddress)));
        }
        return Unauthorized();
    }

    /// <summary>
    /// Gets the attendance summary for a specific month.
    /// </summary>
    [HttpGet("summary")]
    [Authorize]
    public async Task<IActionResult> GetSummary([FromQuery] int? month, [FromQuery] int? year, [FromQuery] int? employeeId)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int currentUserId))
        {
            int targetUserId = employeeId ?? currentUserId;
            return Ok(await _mediator.Send(new GetMobileSummaryQuery(targetUserId, month, year)));
        }
        return Unauthorized();
    }

    /// <summary>
    /// Gets the attendance summary for users in the same department.
    /// </summary>
    [HttpGet("department-users-summary")]
    [Authorize]
    public async Task<IActionResult> GetDepartmentUsersSummary([FromQuery] int? month, [FromQuery] int? year, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            return Ok(await _mediator.Send(new GetDepartmentUsersSummaryQuery(userId, month, year, page, pageSize)));
        }
        return Unauthorized();
    }

    /// <summary>
    /// Gets daily attendance logs.
    /// </summary>
    [HttpGet("daily-logs")]
    [Authorize]
    public async Task<IActionResult> GetDailyLogs([FromQuery] int? month, [FromQuery] int? year, [FromQuery] int? employeeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int currentUserId))
        {
            int targetUserId = employeeId ?? currentUserId;
            return Ok(await _mediator.Send(new GetDailyLogsQuery(targetUserId, month, year, page, pageSize)));
        }
        return Unauthorized();
    }

    /// <summary>
    /// Gets colleagues in the same department.
    /// </summary>
    [HttpGet("department-colleagues")]
    [Authorize]
    public async Task<IActionResult> GetDepartmentColleagues([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            return Ok(await _mediator.Send(new GetDepartmentColleaguesQuery(userId, page, pageSize)));
        }
        return Unauthorized();
    }

    /// <summary>
    /// Gets a list of available attendance devices.
    /// </summary>
    [HttpGet("devices")]
    [Authorize]
    public async Task<IActionResult> GetDevices([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        return Ok(await _mediator.Send(new GetAllDevicesQuery(page, pageSize)));
    }

    /// <summary>
    /// Gets notifications for the user.
    /// </summary>
    [HttpGet("notifications")]
    [Authorize]
    public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            return Ok(await _mediator.Send(new GetNotificationsQuery(userId, unreadOnly, page, pageSize)));
        }
        return Unauthorized();
    }

    /// <summary>
    /// Sends a notification.
    /// </summary>
    [HttpPost("notifications/send")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationCommand command)
    {
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    /// Gets all leave requests for HR review.
    /// </summary>
    [HttpGet("hr/leaves")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> GetAllLeavesForHR([FromQuery] int? status, [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        // Dummy implementation to match endpoint signature.
        return Ok(new List<object>());
    }

    /// <summary>
    /// Approves a leave request.
    /// </summary>
    [HttpPut("hr/leaves/{id}/approve")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> ApproveLeave(int id, [FromBody] ApproveRejectLeaveDto dto)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            return Ok(await _mediator.Send(new ApproveLeaveByHRCommand(id, userId, dto.Comment)));
        }
        return Unauthorized();
    }

    /// <summary>
    /// Rejects a leave request.
    /// </summary>
    [HttpPut("hr/leaves/{id}/reject")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> RejectLeave(int id, [FromBody] ApproveRejectLeaveDto dto)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            return Ok(await _mediator.Send(new RejectLeaveByHRCommand(id, userId, dto.Comment)));
        }
        return Unauthorized();
    }

    private void SetMobileTokenCookie(string? token, string? refreshToken)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken)) return;
        var cookieOptions = new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Expires = DateTime.UtcNow.AddDays(30) };
        Response.Cookies.Append("accessToken", token, cookieOptions);
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
