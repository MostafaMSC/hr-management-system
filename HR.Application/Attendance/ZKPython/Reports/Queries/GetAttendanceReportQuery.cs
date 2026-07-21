using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Reports.Queries;

public record GetAttendanceReportQuery(int Page, int PageSize, string? DeviceIp, string? DateFrom, string? DateTo, string? Search, int? DepartmentId = null) : IRequest<GetAttendanceReportResult>;

public class GetAttendanceReportResult
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public List<AttendanceLogReportResultDto> Data { get; set; } = new();
}

public class AttendanceLogReportResultDto
{
    public string UserID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string Date { get; set; } = string.Empty;
    public string CheckIn { get; set; } = string.Empty;
    public string? CheckOut { get; set; }
}

public class GetAttendanceReportQueryHandler : IRequestHandler<GetAttendanceReportQuery, GetAttendanceReportResult>
{
    private readonly IAttendanceLogRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public GetAttendanceReportQueryHandler(IAttendanceLogRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<GetAttendanceReportResult> Handle(GetAttendanceReportQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        DateTime? dtFrom = DateTime.TryParse(request.DateFrom, out DateTime f) ? DateTime.SpecifyKind(f, DateTimeKind.Utc) : null;
        DateTime? dtTo = DateTime.TryParse(request.DateTo, out DateTime t) ? DateTime.SpecifyKind(t, DateTimeKind.Utc) : null;

        var role = _currentUserService.Role;
        int? filterUserId = null;
        int? filterDeptId = null;

        if (role == Domain.Enums.UserType.Manager)
        {
            filterDeptId = _currentUserService.DepartmentId;
        }
        else if (role == Domain.Enums.UserType.Administrator)
        {
            filterDeptId = request.DepartmentId;
        }
        else if (role == Domain.Enums.UserType.Employee)
        {
            filterUserId = _currentUserService.UserId;
        }

        var (data, total) = await _repository.GetAttendanceReportAsync(
            page,
            pageSize,
            request.DeviceIp,
            dtFrom,
            dtTo,
            request.Search,
            filterUserId,
            filterDeptId);

        var resultData = data.Select(x => new AttendanceLogReportResultDto
        {
            UserID = x.UserID,
            Name = x.Name,
            Department = x.Department,
            Date = x.Date.ToString("yyyy-MM-dd"),
            CheckIn = x.CheckIn.ToString("HH:mm"),
            // Ø¥Ø°Ø§ ÙƒØ§Ù† Ø§Ù„ÙØ±Ù‚ Ø£Ù‚Ù„ Ù…Ù† 30 Ø¯Ù‚ÙŠÙ‚Ø© Ø£Ùˆ Ù†ÙØ³ Ø§Ù„ÙˆÙ‚ØªØŒ Ø§Ø¹ØªØ¨Ø±Ù‡ Ø¯Ø®ÙˆÙ„ ÙÙ‚Ø·
            CheckOut = (x.CheckIn == x.CheckOut || (x.CheckOut - x.CheckIn).TotalMinutes < 30)
                ? null
                : x.CheckOut.ToString("HH:mm")
        }).ToList();

        return new GetAttendanceReportResult
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            TotalPages = (int)Math.Ceiling((double)total / pageSize),
            Data = resultData
        };
    }
}
