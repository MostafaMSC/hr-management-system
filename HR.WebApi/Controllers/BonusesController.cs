using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Bonuses.Commands;
using HR.Application.Bonuses.DTOs;
using HR.Application.Bonuses.Queries;
using HR.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HR.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BonusesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BonusesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst("uid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 0;
    }

    /// <summary>
    /// Creates a new bonus request. Accessible by Managers.
    /// </summary>
    [HttpPost("request")]
    public async Task<IActionResult> CreateBonusRequest([FromBody] CreateBonusRequestCommand command, CancellationToken cancellationToken)
    {
        // Optionally override the RequestingManagerId to ensure security
        var requestCommand = command with { RequestingManagerId = GetCurrentUserId() };
        
        var result = await _mediator.Send(requestCommand, cancellationToken);
        if (result.IsSuccess)
            return Ok(new { success = true, id = result.Value });

        return BadRequest(new { success = false, message = result.Error });
    }

    /// <summary>
    /// Approves a pending bonus request. Accessible by HR/Admin.
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveBonusRequest(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ApproveBonusRequestCommand(id, GetCurrentUserId()), cancellationToken);
        if (result.IsSuccess)
            return Ok(new { success = true });

        return BadRequest(new { success = false, message = result.Error });
    }

    public class RejectBonusRequestDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Rejects a pending bonus request. Accessible by HR/Admin.
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectBonusRequest(int id, [FromBody] RejectBonusRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RejectBonusRequestCommand(id, GetCurrentUserId(), request.Reason), cancellationToken);
        if (result.IsSuccess)
            return Ok(new { success = true });

        return BadRequest(new { success = false, message = result.Error });
    }

    /// <summary>
    /// Retrieves bonus requests based on filters.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<HR.Application.Common.Models.PaginatedResult<BonusRequestDto>>> GetBonusRequests(
        [FromQuery] int? managerId,
        [FromQuery] int? targetUserId,
        [FromQuery] BonusStatus? status,
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBonusRequestsQuery(managerId, targetUserId, status, year, month, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Exports bonus requests to an Excel spreadsheet.
    /// </summary>
    [Authorize(Roles = "Admin,HR")]
    [HttpGet("export")]
    public async Task<IActionResult> ExportBonusRequests(
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] BonusStatus? status,
        CancellationToken cancellationToken)
    {
        var query = new HR.Application.Bonuses.Queries.ExportBonusRequestsQuery(month, year, status);
        var result = await _mediator.Send(query, cancellationToken);
        return File(result.Data, result.ContentType, result.FileName);
    }
}
