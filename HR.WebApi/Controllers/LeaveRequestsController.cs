using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HR.Application.Leaves.Commands;
using HR.Application.Leaves.DTOs;
using HR.Application.Leaves.Queries;
using HR.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HR.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/leave-requests")]
public class LeaveRequestsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeaveRequestsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Creates a new leave request with an optional file attachment.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateLeaveRequest([FromForm] CreateLeaveRequestApiDto dto, IFormFile? attachment = null)
    {
        try
        {
            if (!Enum.TryParse<LeaveType>(dto.Type, ignoreCase: true, out var leaveType))
                return BadRequest($"Invalid leave type '{dto.Type}'. Valid values: hourly, sick, personal, changeShift, etc.");

            DateTime? startDate = null, endDate = null, leaveDate = null;
            TimeSpan? startTime = null, endTime = null;

            if (leaveType == LeaveType.Hourly)
            {
                leaveDate = dto.StartDate;
                if (string.IsNullOrWhiteSpace(dto.FromTime) || !TimeSpan.TryParse(dto.FromTime, out var from))
                    return BadRequest("fromTime is required for hourly leave and must be in HH:mm format.");
                if (string.IsNullOrWhiteSpace(dto.ToTime) || !TimeSpan.TryParse(dto.ToTime, out var to))
                    return BadRequest("toTime is required for hourly leave and must be in HH:mm format.");
                startTime = from;
                endTime = to;
            }
            else
            {
                startDate = dto.StartDate;
                endDate = dto.EndDate ?? dto.StartDate;
            }

            var userId = GetCurrentUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var isManagerRole = userRole == "Manager" || userRole == "Administrator" || userRole == "HR";

            var requestedShiftId = dto.RequestedShiftId <= 0 ? null : dto.RequestedShiftId;

            var command = new CreateLeaveRequestCommand(
                userId,
                leaveType,
                dto.LeaveReason,
                startDate,
                endDate,
                leaveDate,
                startTime,
                endTime,
                dto.Reason,
                isManagerRole,
                requestedShiftId
            );

            var leaveRequestId = await _mediator.Send(command);

            if (attachment != null && attachment.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
                var extension = Path.GetExtension(attachment.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest("File type not allowed. Accepted: PDF, JPG, PNG, DOC, DOCX.");

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "attachments");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = $"{leaveRequestId}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await attachment.CopyToAsync(stream);

                var attachmentUrl = $"/attachments/{fileName}";
                await _mediator.Send(new UploadLeaveAttachmentCommand(leaveRequestId, attachmentUrl));
            }

            return Ok(new { id = leaveRequestId, message = "Leave request created successfully" });
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { return StatusCode(500, $"Internal server error: {ex.Message}"); }
    }

    /// <summary>
    /// Retrieves all leave requests for the currently logged-in employee.
    /// </summary>
    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyLeaveRequests([FromQuery] LeaveStatus? leaveStatus = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetEmployeeLeaveRequestsQuery(GetCurrentUserId(), leaveStatus)
        {
            PageNumber = page,
            PageSize = pageSize
        };
        var requests = await _mediator.Send(query);
        return Ok(requests);
    }

    /// <summary>
    /// Retrieves a specific leave request by its ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetLeaveRequestById(int id)
    {
        var request = await _mediator.Send(new GetLeaveRequestByIdQuery(id));
        if (request == null) return NotFound($"Leave request {id} not found.");
        return Ok(request);
    }

    /// <summary>
    /// Retrieves leave requests submitted by employees within the manager's department.
    /// </summary>
    [HttpGet("department-requests")]
    [Authorize(Roles = "Manager,Administrator,HR")]
    public async Task<IActionResult> GetDepartmentRequests([FromQuery] LeaveStatus? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetDepartmentLeaveRequestsQuery(GetCurrentUserId(), status)
        {
            PageNumber = page,
            PageSize = pageSize
        };
        var requests = await _mediator.Send(query);
        return Ok(requests);
    }

    /// <summary>
    /// Retrieves leave requests that are awaiting final HR approval.
    /// </summary>
    [HttpGet("hr-approvals")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> GetHRApprovals([FromQuery] LeaveStatus? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var queryStatus = status ?? LeaveStatus.AwaitingHRApproval;
        var query = new GetAllLeaveRequestsQuery(queryStatus)
        {
            PageNumber = page,
            PageSize = pageSize
        };
        var requests = await _mediator.Send(query);
        return Ok(requests);
    }

    /// <summary>
    /// Approves a leave request as a Manager.
    /// </summary>
    [HttpPut("{id}/approve-manager")]
    [Authorize(Roles = "Manager,Administrator,HR")]
    public async Task<IActionResult> ApproveLeaveByManager(int id, [FromBody] LeaveApproveRejectDto dto)
    {
        try
        {
            var result = await _mediator.Send(new ApproveLeaveByManagerCommand(id, GetCurrentUserId(), dto.Comment));
            return Ok(new { success = result, message = "Leave request approved by manager" });
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    /// <summary>
    /// Rejects a leave request as a Manager.
    /// </summary>
    [HttpPut("{id}/reject-manager")]
    [Authorize(Roles = "Manager,Administrator,HR")]
    public async Task<IActionResult> RejectLeaveByManager(int id, [FromBody] LeaveApproveRejectDto dto)
    {
        try
        {
            var result = await _mediator.Send(new RejectLeaveByManagerCommand(id, GetCurrentUserId(), dto.Comment));
            return Ok(new { success = result, message = "Leave request rejected by manager" });
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    /// <summary>
    /// Approves a leave request as HR.
    /// </summary>
    [HttpPut("{id}/approve-hr")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> ApproveLeaveByHR(int id, [FromBody] LeaveApproveRejectDto dto)
    {
        try
        {
            var result = await _mediator.Send(new ApproveLeaveByHRCommand(id, GetCurrentUserId(), dto.Comment));
            return Ok(new { success = result, message = "Leave request approved by HR" });
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    /// <summary>
    /// Rejects a leave request as HR.
    /// </summary>
    [HttpPut("{id}/reject-hr")]
    [Authorize(Roles = "Administrator,HR")]
    public async Task<IActionResult> RejectLeaveByHR(int id, [FromBody] LeaveApproveRejectDto dto)
    {
        try
        {
            var result = await _mediator.Send(new RejectLeaveByHRCommand(id, GetCurrentUserId(), dto.Comment));
            return Ok(new { success = result, message = "Leave request rejected by HR" });
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    /// <summary>
    /// Retrieves the current leave balance for the logged-in employee.
    /// </summary>
    [HttpGet("balance")]
    public async Task<IActionResult> GetLeaveBalance()
    {
        var balance = await _mediator.Send(new GetLeaveBalanceQuery(GetCurrentUserId()));
        return Ok(balance);
    }

    /// <summary>
    /// Converts approved overtime hours into regular leave balance.
    /// </summary>
    [HttpPost("convert-overtime")]
    public async Task<IActionResult> ConvertOvertime([FromBody] ConvertOvertimeDto dto)
    {
        try
        {
            var result = await _mediator.Send(new ConvertOvertimeCommand(GetCurrentUserId(), dto.Hours));
            return Ok(new { success = result, message = "Overtime successfully converted" });
        }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    /// <summary>
    /// Uploads a file attachment to an existing leave request.
    /// </summary>
    [HttpPost("{id}/upload-attachment")]
    public async Task<IActionResult> UploadAttachment(int id, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0) return BadRequest("No file provided.");

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest("File type not allowed.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "attachments");
            Directory.CreateDirectory(uploadsFolder);
            var fileName = $"{id}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var attachmentUrl = $"/attachments/{fileName}";
            await _mediator.Send(new UploadLeaveAttachmentCommand(id, attachmentUrl));

            return Ok(new { attachmentUrl, message = "Attachment uploaded successfully" });
        }
        catch (Exception ex) { return StatusCode(500, $"Internal server error: {ex.Message}"); }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedAccessException("User ID not found in token");
        return userId;
    }

    /// <summary>
    /// Exports leave requests to an Excel spreadsheet.
    /// </summary>
    [Authorize(Roles = "Admin,HR")]
    [HttpGet("export")]
    public async Task<IActionResult> ExportLeaveRequests(
        [FromQuery] int? userId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] HR.Domain.Enums.LeaveStatus? status,
        CancellationToken cancellationToken)
    {
        var query = new HR.Application.Leaves.Queries.ExportLeaveRequestsQuery(userId, dateFrom, dateTo, status);
        var result = await _mediator.Send(query, cancellationToken);
        return File(result.Data, result.ContentType, result.FileName);
    }
}
