using System.Data;
using HR.Application.Common.Interfaces;
using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Leaves.Queries;

public record ExportLeaveRequestsQuery(
    int? UserId,
    DateTime? DateFrom,
    DateTime? DateTo,
    LeaveStatus? Status) : IRequest<ExportLeaveRequestsResult>;

public class ExportLeaveRequestsResult
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string FileName { get; set; } = string.Empty;
}

public class ExportLeaveRequestsQueryHandler : IRequestHandler<ExportLeaveRequestsQuery, ExportLeaveRequestsResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IReportExportService _reportExportService;

    public ExportLeaveRequestsQueryHandler(IApplicationDbContext context, IReportExportService reportExportService)
    {
        _context = context;
        _reportExportService = reportExportService;
    }

    public async Task<ExportLeaveRequestsResult> Handle(ExportLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LeaveRequests
            .Include(l => l.UserInfo)
            .AsQueryable();

        if (request.UserId.HasValue)
            query = query.Where(l => l.UserInfoId == request.UserId.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(l => l.StartDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(l => l.EndDate <= request.DateTo.Value);

        if (request.Status.HasValue)
            query = query.Where(l => l.Status == request.Status.Value);

        var leaveRequests = await query
            .OrderByDescending(l => l.StartDate)
            .ToListAsync(cancellationToken);

        var dataTable = new DataTable("LeaveRequests");
        dataTable.Columns.Add("Request ID", typeof(int));
        dataTable.Columns.Add("Employee Name", typeof(string));
        dataTable.Columns.Add("Leave Type", typeof(string));
        dataTable.Columns.Add("Start Date", typeof(string));
        dataTable.Columns.Add("End Date", typeof(string));
        dataTable.Columns.Add("Total Days", typeof(decimal));
        dataTable.Columns.Add("Status", typeof(string));
        dataTable.Columns.Add("Reason", typeof(string));
        dataTable.Columns.Add("HR Comments", typeof(string));

        foreach (var l in leaveRequests)
        {
            var employeeName = l.UserInfo != null ? $"{l.UserInfo.FirstName} {l.UserInfo.LastName}" : "Unknown";

            dataTable.Rows.Add(
                l.Id,
                employeeName,
                l.LeaveType.ToString(),
                l.StartDate.HasValue ? l.StartDate.Value.ToString("yyyy-MM-dd") : string.Empty,
                l.EndDate.HasValue ? l.EndDate.Value.ToString("yyyy-MM-dd") : string.Empty,
                (l.StartDate.HasValue && l.EndDate.HasValue) ? (decimal)((l.EndDate.Value - l.StartDate.Value).Days + 1) : 0m,
                l.Status.ToString(),
                l.Reason ?? string.Empty,
                l.HrComment ?? string.Empty
            );
        }

        var excelBytes = _reportExportService.GenerateExcel(dataTable, "Leave Requests");

        return new ExportLeaveRequestsResult
        {
            Data = excelBytes,
            FileName = $"LeaveRequests_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        };
    }
}
