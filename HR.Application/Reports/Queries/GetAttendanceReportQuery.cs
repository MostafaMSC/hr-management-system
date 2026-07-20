using HR.Application.Common.Interfaces;
using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR.Application.Reports.Queries;

public record GetAttendanceReportQuery(
    int Page, 
    int PageSize, 
    string? DeviceIp, 
    string? DateFrom, 
    string? DateTo, 
    string? Search, 
    int? DepartmentId = null) : IRequest<GetAttendanceReportResult>;

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
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAttendanceReportQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GetAttendanceReportResult> Handle(GetAttendanceReportQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        DateTime? dtFrom = DateTime.TryParse(request.DateFrom, out DateTime f) ? f : null;
        DateTime? dtTo = DateTime.TryParse(request.DateTo, out DateTime t) ? t : null;

        var role = _currentUserService.Role;
        int? filterUserId = null;
        int? filterDeptId = null;

        if (role == UserType.Manager)
        {
            filterDeptId = _currentUserService.DepartmentId;
        }
        else if (role == UserType.Administrator)
        {
            filterDeptId = request.DepartmentId;
        }
        else if (role == UserType.User)
        {
            filterUserId = _currentUserService.UserId;
        }

        var query = _context.DailyAttendanceSummaries
            .Include(s => s.UserInfo)
            .ThenInclude(u => u.Department)
            .AsQueryable();

        if (dtFrom.HasValue)
            query = query.Where(s => s.Date >= dtFrom.Value.Date);

        if (dtTo.HasValue)
            query = query.Where(s => s.Date <= dtTo.Value.Date);

        if (filterDeptId.HasValue)
            query = query.Where(s => s.UserInfo.DepartmentId == filterDeptId.Value);

        if (filterUserId.HasValue)
            query = query.Where(s => s.UserInfoId == filterUserId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(s => 
                (s.UserInfo.FirstName + " " + s.UserInfo.LastName).ToLower().Contains(search) ||
                s.UserInfo.BiometricId.Contains(search) || 
                s.UserInfoId.ToString().Contains(search));
        }

        var total = await query.CountAsync(cancellationToken);

        var data = await query
            .OrderByDescending(s => s.Date)
            .ThenBy(s => s.UserInfo.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                UserID = s.UserInfo.BiometricId ?? s.UserInfoId.ToString(),
                Name = s.UserInfo.FirstName + " " + s.UserInfo.LastName,
                Department = s.UserInfo.Department != null ? s.UserInfo.Department.Name : null,
                Date = s.Date,
                CheckIn = s.TimeIn,
                CheckOut = s.TimeOut
            })
            .ToListAsync(cancellationToken);

        var resultData = data.Select(x => new AttendanceLogReportResultDto 
        {
            UserID = x.UserID,
            Name = x.Name,
            Department = x.Department,
            Date = x.Date.ToString("yyyy-MM-dd"),
            CheckIn = x.CheckIn.HasValue ? x.CheckIn.Value.ToString(@"hh\:mm") : string.Empty,
            CheckOut = (x.CheckIn.HasValue && x.CheckOut.HasValue && (x.CheckOut.Value - x.CheckIn.Value).TotalMinutes < 30) 
                ? null 
                : (x.CheckOut.HasValue ? x.CheckOut.Value.ToString(@"hh\:mm") : null)
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
