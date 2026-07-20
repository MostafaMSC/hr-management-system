using HR.Application.Sections.Commands;
using HR.Application.Sections.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SectionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SectionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "departmentId" })]
    public async Task<IActionResult> GetSections([FromQuery] int? departmentId = null)
    {
        var sections = await _mediator.Send(new GetSectionsQuery(departmentId));
        return Ok(sections);
    }

    [HttpGet("{id}")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetSectionById(int id)
    {
        var section = await _mediator.Send(new GetSectionByIdQuery(id));
        if (section == null)
        {
            return NotFound($"Section with ID {id} not found.");
        }
        return Ok(section);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSection([FromBody] CreateSectionCommand command)
    {
        try
        {
            var section = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetSectionById), new { id = section.Id }, section);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSection(int id, [FromBody] UpdateSectionRequest request)
    {
        try
        {
            var command = new UpdateSectionCommand(id, request.Name, request.DepartmentId, request.Description);
            var section = await _mediator.Send(command);
            return Ok(section);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSection(int id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteSectionCommand(id));
            return Ok(new { success = result, message = "Section deleted successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record UpdateSectionRequest(string Name, int DepartmentId, string? Description = null);
