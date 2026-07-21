using HR.Application.Attendance.Shifts.Commands;
using HR.Application.Attendance.Shifts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/shifts")]
public class AttendanceShiftsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AttendanceShiftsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all attendance shifts in the system.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetShifts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetShiftsQuery(page, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific attendance shift by its unique ID.
    /// </summary>
    /// <param name="id">The ID of the shift</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetShiftById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetShiftByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new attendance shift. Requires Administrator or HR privileges.
    /// </summary>
    /// <param name="command">The shift creation details</param>
    [Authorize(Roles = "Administrator,HR")]
    [HttpPost]
    public async Task<IActionResult> CreateShift([FromBody] CreateAttendanceShiftCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetShiftById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing attendance shift. Requires Administrator or HR privileges.
    /// </summary>
    /// <param name="id">The ID of the shift to update</param>
    /// <param name="command">The updated shift details</param>
    [Authorize(Roles = "Administrator,HR")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateShift(int id, [FromBody] UpdateAttendanceShiftCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID in URL must match ID in request body.");

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes an attendance shift by its ID. Requires Administrator or HR privileges.
    /// </summary>
    /// <param name="id">The ID of the shift to delete</param>
    [Authorize(Roles = "Administrator,HR")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteShift(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteAttendanceShiftCommand(id), cancellationToken);
        return NoContent();
    }
}
