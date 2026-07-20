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

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] MobileLoginRequest request)
    {
        var response = await _mediator.Send(new MobileLoginCommand(request.Email, request.Password, request.FcmToken));
        SetMobileTokenCookie(response.AccessToken, response.RefreshToken);
        return Ok(response);
    }

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

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("accessToken");
        Response.Cookies.Delete("refreshToken");
        return Ok(new { message = "تم تسجيل الخروج بنجاح" });
    }

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

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        // Dummy implementation to match endpoint signature.
        return Ok(new { message = "OTP Verified" });
    }

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

    [HttpGet("department-users-summary")]
    [Authorize]
    public async Task<IActionResult> GetDepartmentUsersSummary([FromQuery] int? month, [FromQuery] int? year)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            return Ok(await _mediator.Send(new GetDepartmentUsersSummaryQuery(userId, month, year)));
        }
        return Unauthorized();
    }

    [HttpGet("daily-logs")]
    [Authorize]
    public async Task<IActionResult> GetDailyLogs([FromQuery] int? month, [FromQuery] int? year, [FromQuery] int? employeeId)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int currentUserId))
        {
            int targetUserId = employeeId ?? currentUserId;
            return Ok(await _mediator.Send(new GetDailyLogsQuery(targetUserId, month, year)));
        }
        return Unauthorized();
    }

    [HttpGet("department-colleagues")]
    [Authorize]
    public async Task<IActionResult> GetDepartmentColleagues()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            return Ok(await _mediator.Send(new GetDepartmentColleaguesQuery(userId)));
        }
        return Unauthorized();
    }

    [HttpGet("devices")]
    [Authorize]
    public async Task<IActionResult> GetDevices()
    {
        return Ok(await _mediator.Send(new GetAllDevicesQuery()));
    }

    [HttpGet("notifications")]
    [Authorize]
    public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false, [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId))
        {
            return Ok(await _mediator.Send(new GetNotificationsQuery(userId, unreadOnly)));
        }
        return Unauthorized();
    }

    [HttpPost("notifications/send")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationCommand command)
    {
        return Ok(await _mediator.Send(command));
    }

    [HttpGet("hr/leaves")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> GetAllLeavesForHR([FromQuery] int? status, [FromQuery] int? pageNumber = null, [FromQuery] int? pageSize = null)
    {
        // Dummy implementation to match endpoint signature.
        return Ok(new List<object>());
    }

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
