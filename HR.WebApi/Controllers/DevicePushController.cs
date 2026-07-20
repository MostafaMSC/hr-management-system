using HR.Application.Attendance.Commands;
using HR.Application.Attendance.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HR.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicePushController : ControllerBase
{
    private readonly IMediator _mediator;

    public DevicePushController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Endpoint for ADMS/Webhooks to push real-time attendance logs directly to the system.
    /// </summary>
    /// <param name="logs">List of attendance logs to push</param>
    [HttpPost("logs")]
    public async Task<IActionResult> PushLogs([FromBody] List<AttendanceLogDto> logs, CancellationToken cancellationToken)
    {
        if (logs == null || !logs.Any())
        {
            return BadRequest("No logs provided.");
        }

        int processedCount = 0;

        foreach (var log in logs)
        {
            var command = new ProcessAttendanceLogCommand { Log = log };
            var success = await _mediator.Send(command, cancellationToken);

            if (success)
            {
                processedCount++;
            }
        }

        return Ok(new { Message = $"Processed {processedCount} out of {logs.Count} logs successfully." });
    }
}
