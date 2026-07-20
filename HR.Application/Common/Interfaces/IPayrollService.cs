using HR.Application.Payroll.DTOs;

namespace HR.Application.Common.Interfaces;

public interface IPayrollService
{
    /// <summary>
    /// Parse the Excel file, match employees to DB users, return a preview
    /// </summary>
    Task<SalarySheetPreviewResponse> PreviewSalarySheetAsync(Stream excelStream, CancellationToken cancellationToken);

    /// <summary>
    /// Generate PDF slips and send emails for the confirmed entries
    /// </summary>
    Task<SendSalarySlipsResponse> SendSalarySlipsAsync(SendSalarySlipsRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Generate a combined PDF of all payslips, optionally filtered by department.
    /// Used by HR to download copies.
    /// </summary>
    Task<byte[]> GenerateHrCopyAsync(SendSalarySlipsRequest request, string? departmentFilter, CancellationToken cancellationToken);

    /// <summary>
    /// Legacy: parse + send in one shot (kept for backward compat)
    /// </summary>
    Task ProcessSalarySheetAsync(Stream excelStream, CancellationToken cancellationToken);
}
