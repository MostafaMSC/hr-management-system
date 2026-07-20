using System.Data;
using HR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Attendance.ZKPython.Logs.Queries;

public record ExportAttendanceLogsQuery(
    int? UserId,
    string? DeviceIp,
    DateTime? DateFrom,
    DateTime? DateTo) : IRequest<ExportAttendanceLogsResult>;

public class ExportAttendanceLogsResult
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string FileName { get; set; } = string.Empty;
}

public class ExportAttendanceLogsQueryHandler : IRequestHandler<ExportAttendanceLogsQuery, ExportAttendanceLogsResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IReportExportService _reportExportService;

    public ExportAttendanceLogsQueryHandler(IApplicationDbContext context, IReportExportService reportExportService)
    {
        _context = context;
        _reportExportService = reportExportService;
    }

    public async Task<ExportAttendanceLogsResult> Handle(ExportAttendanceLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AttendanceLogs
            .Include(l => l.UserInfo)
            .AsQueryable();

        if (request.UserId.HasValue)
            query = query.Where(l => l.UserInfoId == request.UserId.Value);

        if (!string.IsNullOrEmpty(request.DeviceIp))
            query = query.Where(l => l.DeviceIP == request.DeviceIp);

        if (request.DateFrom.HasValue)
            query = query.Where(l => l.PunchTime >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(l => l.PunchTime <= request.DateTo.Value);

        var logs = await query
            .OrderByDescending(l => l.PunchTime)
            .ToListAsync(cancellationToken);

        var dataTable = new DataTable("AttendanceLogs");
        dataTable.Columns.Add("Log ID", typeof(int));
        dataTable.Columns.Add("Employee Name", typeof(string));
        dataTable.Columns.Add("Biometric ID", typeof(string));
        dataTable.Columns.Add("Punch Time", typeof(string));
        dataTable.Columns.Add("Punch Type", typeof(string));
        dataTable.Columns.Add("Device IP", typeof(string));
        dataTable.Columns.Add("Is Mobile Punch", typeof(bool));

        foreach (var l in logs)
        {
            var employeeName = l.UserInfo != null ? $"{l.UserInfo.FirstName} {l.UserInfo.LastName}" : "Unknown";
            var biometricId = l.UserInfo?.BiometricId ?? string.Empty;

            dataTable.Rows.Add(
                l.Id,
                employeeName,
                biometricId,
                l.PunchTime.ToString("yyyy-MM-dd HH:mm:ss"),
                l.PunchType.ToString(),
                l.DeviceIP ?? "N/A",
                l.LogsType.ToString() == "Mobile"
            );
        }

        var excelBytes = _reportExportService.GenerateExcel(dataTable, "Attendance Logs");

        return new ExportAttendanceLogsResult
        {
            Data = excelBytes,
            FileName = $"AttendanceLogs_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        };
    }
}
