using HR.Application.Payroll.Commands;
using HR.Application.Payroll.DTOs;
using HR.Application.Payroll.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HR.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminPolicy")]
public class PayrollController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PayrollController> _logger;

    public PayrollController(IMediator mediator, ILogger<PayrollController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Step 1: Upload Excel → get preview of matched employees
    /// </summary>
    [HttpPost("preview-salary-sheet")]
    [ProducesResponseType(typeof(SalarySheetPreviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PreviewSalarySheet(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No valid file uploaded." });

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only .xlsx files are supported." });

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _mediator.Send(new PreviewSalarySheetCommand(stream), cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to preview salary sheet.");
            return StatusCode(500, new { message = "An error occurred while parsing the salary sheet." });
        }
    }

    /// <summary>
    /// Step 2: Send salary slips for confirmed matches
    /// </summary>
    [HttpPost("send-salary-slips")]
    [ProducesResponseType(typeof(SendSalarySlipsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendSalarySlips([FromBody] SendSalarySlipsRequest request, CancellationToken cancellationToken)
    {
        if (request?.Entries == null || request.Entries.Count == 0)
            return BadRequest(new { message = "No entries provided." });

        try
        {
            var result = await _mediator.Send(new SendSalarySlipsCommand(request), cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send salary slips.");
            return StatusCode(500, new { message = "An error occurred while sending salary slips." });
        }
    }

    /// <summary>
    /// HR COPY: Generate combined multi-page PDF filtered by dept
    /// </summary>
    [HttpPost("generate-hr-copy")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateHrCopy([FromBody] SendSalarySlipsRequest request, [FromQuery] string? department, CancellationToken cancellationToken)
    {
        if (request?.Entries == null || request.Entries.Count == 0)
            return BadRequest(new { message = "No entries provided." });

        try
        {
            var pdfBytes = await _mediator.Send(new GenerateHrCopyQuery(request, department), cancellationToken);
            return File(pdfBytes, "application/pdf", $"HR_Copy_Salary_Slips_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate HR copy PDF.");
            return StatusCode(500, new { message = "An error occurred while generating the PDF." });
        }
    }

    /// <summary>
    /// Legacy: one-shot process (kept for backward compatibility)
    /// </summary>
    [HttpPost("process-salary-sheet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessSalarySheet(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No valid file uploaded." });

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only .xlsx files are supported." });

        try
        {
            using var stream = file.OpenReadStream();
            await _mediator.Send(new ProcessSalarySheetCommand(stream), cancellationToken);
            return Ok(new { message = "Salary sheet processed and emails pushed to the queue successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process salary sheet.");
            return StatusCode(500, new { message = "An error occurred while processing the salary sheet." });
        }
    }
}
