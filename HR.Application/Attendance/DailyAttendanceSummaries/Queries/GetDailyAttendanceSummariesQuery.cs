using HR.Application.Common.Interfaces;
using HR.Application.Attendance.DailyAttendanceSummaries.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Attendance.DailyAttendanceSummaries.Queries;

public record GetDailyAttendanceSummariesQuery(
    int? UserId, 
    DateTime? DateFrom, 
    DateTime? DateTo, 
    int Page = 1, 
    int PageSize = 20, 
    string? SortBy = null, 
    string? SortDirection = "asc") : IRequest<GetDailyAttendanceSummariesResult>;

public class GetDailyAttendanceSummariesResult
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public List<DailyAttendanceSummaryDto> Data { get; set; } = new();
}

public class GetDailyAttendanceSummariesQueryHandler : IRequestHandler<GetDailyAttendanceSummariesQuery, GetDailyAttendanceSummariesResult>
{
    private readonly IApplicationDbContext _context;

    public GetDailyAttendanceSummariesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetDailyAttendanceSummariesResult> Handle(GetDailyAttendanceSummariesQuery request, CancellationToken cancellationToken)
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

        var total = await query.CountAsync(cancellationToken);

        bool isDesc = request.SortDirection?.ToLower() == "desc" || request.SortDirection?.ToLower() == "descending";
        
        query = request.SortBy?.ToLower() switch
        {
            "date" => isDesc ? query.OrderByDescending(s => s.Date) : query.OrderBy(s => s.Date),
            "username" => isDesc ? query.OrderByDescending(s => s.UserInfo.Username) : query.OrderBy(s => s.UserInfo.Username),
            "employeeid" => isDesc ? query.OrderByDescending(s => s.UserInfo.BiometricId) : query.OrderBy(s => s.UserInfo.BiometricId),
            "shiftname" => isDesc ? query.OrderByDescending(s => s.UserInfo.AttendanceShift!.Name) : query.OrderBy(s => s.UserInfo.AttendanceShift!.Name),
            "timein" => isDesc ? query.OrderByDescending(s => s.TimeIn) : query.OrderBy(s => s.TimeIn),
            "timeout" => isDesc ? query.OrderByDescending(s => s.TimeOut) : query.OrderBy(s => s.TimeOut),
            "delayminutes" => isDesc ? query.OrderByDescending(s => s.DelayMinutes) : query.OrderBy(s => s.DelayMinutes),
            "deducteddays" => isDesc ? query.OrderByDescending(s => s.DeductedDays) : query.OrderBy(s => s.DeductedDays),
            "overtimehours" => isDesc ? query.OrderByDescending(s => s.OvertimeMinutes) : query.OrderBy(s => s.OvertimeMinutes),
            "status" => isDesc ? query.OrderByDescending(s => s.Status) : query.OrderBy(s => s.Status),
            _ => query.OrderByDescending(s => s.Date)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new DailyAttendanceSummaryDto
            {
                Id = s.Id,
                UserId = s.UserInfoId,
                Username = s.UserInfo != null ? s.UserInfo.Username : string.Empty,
                EmployeeId = (s.UserInfo != null && s.UserInfo.BiometricId != null) ? s.UserInfo.BiometricId : string.Empty,
                Date = s.Date.ToString("yyyy-MM-dd"),
                ShiftId = s.UserInfo != null ? s.UserInfo.AttendanceShiftId : null,
                ShiftName = (s.UserInfo != null && s.UserInfo.AttendanceShift != null) ? s.UserInfo.AttendanceShift.Name : "None",
                TimeIn = s.TimeIn.HasValue ? s.TimeIn.Value.ToString(@"hh\:mm") : null,
                TimeOut = s.TimeOut.HasValue ? s.TimeOut.Value.ToString(@"hh\:mm") : null,
                DelayMinutes = s.DelayMinutes,
                DeductedDays = s.DeductedDays,
                OvertimeHours = s.OvertimeMinutes / 60.0m,
                Status = s.Status.ToString(),
                IsDeductionApplied = s.IsDeductionApplied
            })
            .ToListAsync(cancellationToken);

        return new GetDailyAttendanceSummariesResult
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Total = total,
            TotalPages = (int)Math.Ceiling((double)total / request.PageSize),
            Data = items
        };
    }
}
