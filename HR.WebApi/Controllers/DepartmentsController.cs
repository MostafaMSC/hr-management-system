using HR.Application.Departments.Commands;
using HR.Application.Departments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DepartmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetDepartments(CancellationToken cancellationToken)
    {
        var departments = await _mediator.Send(new GetDepartmentsQuery(), cancellationToken);
        return Ok(departments);
    }

    [HttpGet("{id}")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetDepartmentById(int id, CancellationToken cancellationToken)
    {
        var department = await _mediator.Send(new GetDepartmentByIdQuery(id), cancellationToken);
        return Ok(department);
    }

    [HttpPost]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var department = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetDepartmentById), new { id = department.Id }, department);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID in URL must match ID in request body.");

        try
        {
            var department = await _mediator.Send(command, cancellationToken);
            return Ok(department);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDepartment(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new DeleteDepartmentCommand(id), cancellationToken);
            return Ok(new { success = result, message = "Department deleted successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
