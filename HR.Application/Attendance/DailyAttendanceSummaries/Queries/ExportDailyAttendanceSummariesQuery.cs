using System.Data;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.DailyAttendanceSummaries.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Attendance.DailyAttendanceSummaries.Queries;

public record ExportDailyAttendanceSummariesQuery(
    int? UserId, 
    DateTime? DateFrom, 
    DateTime? DateTo) : IRequest<ExportDailyAttendanceSummariesResult>;

public class ExportDailyAttendanceSummariesResult
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public string FileName { get; set; } = string.Empty;
}

public class ExportDailyAttendanceSummariesQueryHandler : IRequestHandler<ExportDailyAttendanceSummariesQuery, ExportDailyAttendanceSummariesResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IReportExportService _reportExportService;

    public ExportDailyAttendanceSummariesQueryHandler(IApplicationDbContext context, IReportExportService reportExportService)
    {
        _context = context;
        _reportExportService = reportExportService;
    }

    public async Task<ExportDailyAttendanceSummariesResult> Handle(ExportDailyAttendanceSummariesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DailyAttendanceSummaries
            .Include(s => s.UserInfo)
            .ThenInclude(u => u.AttendanceShift)
            .AsQueryable();

        if (request.UserId.HasValue)
        {
            query = query.Where(s => s.UserInfoId == request.UserId.Value);
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(s => s.Date >= request.DateFrom.Value.Date);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(s => s.Date <= request.DateTo.Value.Date);
        }

        var items = await query
            .OrderByDescending(s => s.Date)
            .Select(s => new DailyEvaluationExportDto
            {
                Id = s.Id,
                Username = s.UserInfo != null ? s.UserInfo.Username : string.Empty,
                EmployeeId = (s.UserInfo != null && s.UserInfo.BiometricId != null) ? s.UserInfo.BiometricId : string.Empty,
                Date = s.Date.ToString("yyyy-MM-dd"),
                ShiftName = (s.UserInfo != null && s.UserInfo.AttendanceShift != null) ? s.UserInfo.AttendanceShift.Name : "None",
                TimeIn = s.TimeIn.HasValue ? s.TimeIn.Value.ToString(@"hh\:mm") : string.Empty,
                TimeOut = s.TimeOut.HasValue ? s.TimeOut.Value.ToString(@"hh\:mm") : string.Empty,
                DelayMinutes = s.DelayMinutes,
                DeductedDays = s.DeductedDays,
                OvertimeHours = s.OvertimeMinutes / 60.0m,
                Status = s.Status.ToString(),
                IsDeductionApplied = s.IsDeductionApplied
            })
            .ToListAsync(cancellationToken);

        var dataTable = new DataTable("DailyEvaluations");
        dataTable.Columns.Add("ID", typeof(int));
        dataTable.Columns.Add("Username", typeof(string));
        dataTable.Columns.Add("Employee ID", typeof(string));
        dataTable.Columns.Add("Date", typeof(string));
        dataTable.Columns.Add("Shift Name", typeof(string));
        dataTable.Columns.Add("Time In", typeof(string));
        dataTable.Columns.Add("Time Out", typeof(string));
        dataTable.Columns.Add("Delay (Mins)", typeof(int));
        dataTable.Columns.Add("Deducted Days", typeof(decimal));
        dataTable.Columns.Add("Overtime (Hours)", typeof(decimal));
        dataTable.Columns.Add("Status", typeof(string));
        dataTable.Columns.Add("Deduction Applied", typeof(bool));

        foreach (var item in items)
        {
            dataTable.Rows.Add(
                item.Id,
                item.Username,
                item.EmployeeId,
                item.Date,
                item.ShiftName,
                item.TimeIn,
                item.TimeOut,
                item.DelayMinutes,
                item.DeductedDays,
                item.OvertimeHours,
                item.Status,
                item.IsDeductionApplied
            );
        }

        var excelBytes = _reportExportService.GenerateExcel(dataTable, "Daily Evaluations");

        return new ExportDailyAttendanceSummariesResult
        {
            Data = excelBytes,
            FileName = $"DailyEvaluation_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        };
    }
}
