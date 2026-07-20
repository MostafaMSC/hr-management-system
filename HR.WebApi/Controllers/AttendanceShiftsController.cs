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

    [HttpGet]
    public async Task<IActionResult> GetShifts(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetShiftsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetShiftById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetShiftByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Administrator,HR")]
    [HttpPost]
    public async Task<IActionResult> CreateShift([FromBody] CreateAttendanceShiftCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetShiftById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Administrator,HR")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateShift(int id, [FromBody] UpdateAttendanceShiftCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID in URL must match ID in request body.");

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Administrator,HR")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteShift(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteAttendanceShiftCommand(id), cancellationToken);
        return NoContent();
    }
}
