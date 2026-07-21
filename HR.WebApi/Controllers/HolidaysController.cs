using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using HR.Application.Holidays.Commands;
using HR.Application.Holidays.Queries;
using HR.Application.Holidays.DTOs;

namespace HR.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HolidaysController : ControllerBase
{
    private readonly IMediator _mediator;

    public HolidaysController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all registered holidays.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHolidays(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var holidays = await _mediator.Send(new GetHolidaysQuery(page, pageSize));
        return Ok(holidays);
    }

    /// <summary>
    /// Retrieves a specific holiday by ID.
    /// </summary>
    /// <param name="id">The internal holiday ID</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetHolidayById(int id)
    {
        var holiday = await _mediator.Send(new GetHolidayByIdQuery(id));
        if (holiday == null)
            return NotFound($"Holiday with ID {id} not found.");

        return Ok(holiday);
    }

    /// <summary>
    /// Creates one or multiple holidays and sends a push notification to all active users.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateHoliday([FromBody] CreateHolidayRequest request)
    {
        try
        {
            var id = await _mediator.Send(new CreateHolidayCommand
            {
                Name = request.Name,
                Date = request.Date,
                EndDate = request.EndDate
            });

            // To be entirely RESTful, we fetch the newly created (first) holiday
            var holiday = await _mediator.Send(new GetHolidayByIdQuery(id));
            return CreatedAtAction(nameof(GetHolidayById), new { id }, holiday);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing holiday.
    /// </summary>
    /// <param name="id">The ID of the holiday to update</param>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHoliday(int id, [FromBody] UpdateHolidayRequest request)
    {
        try
        {
            await _mediator.Send(new UpdateHolidayCommand
            {
                Id = id,
                Name = request.Name,
                Date = request.Date
            });

            var holiday = await _mediator.Send(new GetHolidayByIdQuery(id));
            return Ok(holiday);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deletes an existing holiday.
    /// </summary>
    /// <param name="id">The ID of the holiday to delete</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHoliday(int id)
    {
        try
        {
            await _mediator.Send(new DeleteHolidayCommand { Id = id });
            return Ok(new { success = true, message = "Holiday deleted successfully." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    public class CreateHolidayRequest
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class UpdateHolidayRequest
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
