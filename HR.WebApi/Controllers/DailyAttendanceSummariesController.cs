using HR.Application.Attendance.DailyAttendanceSummaries.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/daily-attendance-summaries")]
public class DailyAttendanceSummariesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DailyAttendanceSummariesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSummaries(
        [FromQuery] int? userId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        var query = new GetDailyAttendanceSummariesQuery(
            userId, dateFrom, dateTo, page, pageSize, sortBy, sortDirection);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportSummaries(
        [FromQuery] int? userId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        CancellationToken cancellationToken = default)
    {
        var query = new ExportDailyAttendanceSummariesQuery(userId, dateFrom, dateTo);
        var result = await _mediator.Send(query, cancellationToken);
        
        return File(result.Data, result.ContentType, result.FileName);
    }
}
