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

    /// <summary>
    /// Retrieves paginated daily attendance summaries with optional filtering.
    /// </summary>
    /// <param name="userId">Optional user ID filter</param>
    /// <param name="dateFrom">Optional start date filter</param>
    /// <param name="dateTo">Optional end date filter</param>
    /// <param name="page">Page number (default 1)</param>
    /// <param name="pageSize">Items per page (default 20)</param>
    /// <param name="sortBy">Field to sort by</param>
    /// <param name="sortDirection">Direction of sort (asc/desc)</param>
    [HttpGet]
    public async Task<IActionResult> GetSummaries(
        [FromQuery] int? userId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc",
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDailyAttendanceSummariesQuery(
            userId, dateFrom, dateTo, page, pageSize, sortBy, sortDirection, search);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Exports the attendance summaries to an Excel file.
    /// </summary>
    /// <param name="userId">Optional user ID filter</param>
    /// <param name="dateFrom">Optional start date filter</param>
    /// <param name="dateTo">Optional end date filter</param>
    [HttpGet("export")]
    public async Task<IActionResult> ExportSummaries(
        [FromQuery] int? userId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new ExportDailyAttendanceSummariesQuery(userId, dateFrom, dateTo, search);
        var result = await _mediator.Send(query, cancellationToken);

        return File(result.Data, result.ContentType, result.FileName);
    }
}
