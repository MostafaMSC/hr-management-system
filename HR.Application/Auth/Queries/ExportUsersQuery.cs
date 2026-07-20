using System.Data;
using HR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Auth.Queries;

public record ExportUsersQuery() : IRequest<ExportUsersResult>;

public class ExportUsersResult
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string FileName { get; set; } = string.Empty;
}

public class ExportUsersQueryHandler : IRequestHandler<ExportUsersQuery, ExportUsersResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IReportExportService _reportExportService;

    public ExportUsersQueryHandler(IApplicationDbContext context, IReportExportService reportExportService)
    {
        _context = context;
        _reportExportService = reportExportService;
    }

    public async Task<ExportUsersResult> Handle(ExportUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.UserInfos
            .Include(u => u.Department)
            .Include(u => u.Section)
            .Include(u => u.AttendanceShift)
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync(cancellationToken);

        var dataTable = new DataTable("Users");
        dataTable.Columns.Add("ID", typeof(int));
        dataTable.Columns.Add("Employee/Biometric ID", typeof(string));
        dataTable.Columns.Add("First Name", typeof(string));
        dataTable.Columns.Add("Last Name", typeof(string));
        dataTable.Columns.Add("Username", typeof(string));
        dataTable.Columns.Add("Email", typeof(string));
        dataTable.Columns.Add("Role", typeof(string));
        dataTable.Columns.Add("Department", typeof(string));
        dataTable.Columns.Add("Section", typeof(string));
        dataTable.Columns.Add("Shift", typeof(string));
        dataTable.Columns.Add("Date of Joining", typeof(string));
        dataTable.Columns.Add("Account Status", typeof(string));

        foreach (var user in users)
        {
            dataTable.Rows.Add(
                user.Id,
                user.BiometricId ?? string.Empty,
                user.FirstName,
                user.LastName,
                user.Username,
                user.Email,
                user.Role.ToString(),
                user.Department?.Name ?? "None",
                user.Section?.Name ?? "None",
                user.AttendanceShift?.Name ?? "None",
                user.DateOfJoining.ToString("yyyy-MM-dd"),
                user.AccountStatus ?? "Active"
            );
        }

        var excelBytes = _reportExportService.GenerateExcel(dataTable, "Users");

        return new ExportUsersResult
        {
            Data = excelBytes,
            FileName = $"Users_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        };
    }
}
