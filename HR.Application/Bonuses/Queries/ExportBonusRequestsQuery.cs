using System.Data;
using HR.Application.Common.Interfaces;
using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Bonuses.Queries;

public record ExportBonusRequestsQuery(
    int? Month,
    int? Year,
    BonusStatus? Status) : IRequest<ExportBonusRequestsResult>;

public class ExportBonusRequestsResult
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string FileName { get; set; } = string.Empty;
}

public class ExportBonusRequestsQueryHandler : IRequestHandler<ExportBonusRequestsQuery, ExportBonusRequestsResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IReportExportService _reportExportService;

    public ExportBonusRequestsQueryHandler(IApplicationDbContext context, IReportExportService reportExportService)
    {
        _context = context;
        _reportExportService = reportExportService;
    }

    public async Task<ExportBonusRequestsResult> Handle(ExportBonusRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BonusRequests
            .Include(b => b.TargetUser)
            .Include(b => b.RequestingManager)
            .AsQueryable();

        if (request.Month.HasValue)
            query = query.Where(b => b.Month == request.Month.Value);

        if (request.Year.HasValue)
            query = query.Where(b => b.Year == request.Year.Value);

        if (request.Status.HasValue)
            query = query.Where(b => b.Status == request.Status.Value);

        var bonusRequests = await query
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        var dataTable = new DataTable("BonusRequests");
        dataTable.Columns.Add("Bonus ID", typeof(int));
        dataTable.Columns.Add("Employee Name", typeof(string));
        dataTable.Columns.Add("Manager Name", typeof(string));
        dataTable.Columns.Add("Month/Year", typeof(string));
        dataTable.Columns.Add("Bonus Type", typeof(string));
        dataTable.Columns.Add("Amount/Percentage", typeof(decimal));
        dataTable.Columns.Add("Status", typeof(string));
        dataTable.Columns.Add("Requested At", typeof(string));
        dataTable.Columns.Add("Reason/Comments", typeof(string));

        foreach (var b in bonusRequests)
        {
            var employeeName = b.TargetUser != null ? $"{b.TargetUser.FirstName} {b.TargetUser.LastName}" : "Unknown";
            var managerName = b.RequestingManager != null ? $"{b.RequestingManager.FirstName} {b.RequestingManager.LastName}" : "Unknown";

            dataTable.Rows.Add(
                b.Id,
                employeeName,
                managerName,
                $"{b.Month}/{b.Year}",
                b.Type.ToString(),
                b.Value,
                b.Status.ToString(),
                b.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                b.Reason ?? string.Empty
            );
        }

        var excelBytes = _reportExportService.GenerateExcel(dataTable, "Bonus Requests");

        return new ExportBonusRequestsResult
        {
            Data = excelBytes,
            FileName = $"BonusRequests_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        };
    }
}
