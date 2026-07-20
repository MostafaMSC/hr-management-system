using HR.Application.Settings.Commands;
using HR.Application.Settings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves global work settings.
    /// </summary>
    [HttpGet("work-settings")]
    public async Task<IActionResult> GetWorkSettings(CancellationToken cancellationToken)
    {
        var settings = await _mediator.Send(new GetWorkSettingsQuery(), cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Updates global work settings.
    /// </summary>
    [Authorize(Policy = "AdminPolicy")]
    [HttpPut("work-settings")]
    public async Task<IActionResult> UpdateWorkSettings([FromBody] Dictionary<string, object> settings, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateWorkSettingsCommand(settings), cancellationToken);
        return Ok(new { message = "Settings updated successfully" });
    }
}
