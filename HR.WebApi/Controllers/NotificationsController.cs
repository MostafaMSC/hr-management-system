using HR.Application.Common.Interfaces;
using HR.Application.Notifications.Commands;
using HR.Application.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "EmployeePolicy")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get all notifications for the current user
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int? pageNumber = null,
        [FromQuery] int? pageSize = null)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Unauthorized(new { message = "User not authenticated." });

        var query = new GetUserNotificationsQuery(userId.Value, unreadOnly, pageNumber, pageSize);
        var notifications = await _mediator.Send(query);
        return Ok(notifications);
    }

    /// <summary>
    /// Get unread notification count for current user
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Unauthorized(new { message = "User not authenticated." });

        var query = new GetUnreadCountQuery(userId.Value);
        var count = await _mediator.Send(query);
        return Ok(new { count });
    }

    /// <summary>
    /// Mark a specific notification as read
    /// </summary>
    [HttpPut("{id}/mark-read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Unauthorized(new { message = "User not authenticated." });

        try
        {
            var command = new MarkNotificationAsReadCommand
            {
                NotificationId = id,
                UserId = userId.Value
            };

            var result = await _mediator.Send(command);
            return Ok(new { success = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Mark all notifications as read for the current user
    /// </summary>
    [HttpPut("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Unauthorized(new { message = "User not authenticated." });

        var command = new MarkAllNotificationsAsReadCommand
        {
            UserId = userId.Value
        };

        var result = await _mediator.Send(command);
        return Ok(new { success = result });
    }

    /// <summary>
    /// Delete a specific notification
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Unauthorized(new { message = "User not authenticated." });

        try
        {
            var command = new DeleteNotificationCommand
            {
                NotificationId = id,
                UserId = userId.Value
            };

            var result = await _mediator.Send(command);
            return Ok(new { success = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Send a notification (global broadcast or targeted user)
    /// </summary>
    [HttpPost("send")]
    [Authorize(Policy = "HRPolicy")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(new { success = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
