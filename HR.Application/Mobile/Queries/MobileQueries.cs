using HR.Application.Common.Interfaces;
using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Mobile.Queries;

// --- DTOs ---
public class MobileSummaryDto
{
    public int WorkedDays { get; set; }
    public int LateDays { get; set; }
    public int AbsentDays { get; set; }
    public object? LeaveBalances { get; set; }
}

// --- Queries ---
public record GetMobileSummaryQuery(int UserId, int? Month, int? Year) : IRequest<MobileSummaryDto>;
public record GetDepartmentUsersSummaryQuery(int UserId, int? Month, int? Year, int Page = 1, int PageSize = 10) : IRequest<object>;
public record GetDailyLogsQuery(int UserId, int? Month, int? Year, int Page = 1, int PageSize = 10) : IRequest<object>;
public record GetDepartmentColleaguesQuery(int UserId, int Page = 1, int PageSize = 10) : IRequest<object>;
public record GetNotificationsQuery(int UserId, bool UnreadOnly, int Page = 1, int PageSize = 10) : IRequest<object>;
public record GetAllDevicesQuery(int Page = 1, int PageSize = 10) : IRequest<object>;

// --- Handlers ---
public class MobileQueriesHandler :
    IRequestHandler<GetMobileSummaryQuery, MobileSummaryDto>,
    IRequestHandler<GetDepartmentUsersSummaryQuery, object>,
    IRequestHandler<GetDailyLogsQuery, object>,
    IRequestHandler<GetDepartmentColleaguesQuery, object>,
    IRequestHandler<GetNotificationsQuery, object>,
    IRequestHandler<GetAllDevicesQuery, object>
{
    private readonly IApplicationDbContext _context;

    public MobileQueriesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MobileSummaryDto> Handle(GetMobileSummaryQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var targetMonth = request.Month ?? now.Month;
        var targetYear = request.Year ?? now.Year;

        var logs = await _context.AttendanceLogs
            .Where(l => l.UserInfoId == request.UserId && l.PunchTime.Month == targetMonth && l.PunchTime.Year == targetYear)
            .ToListAsync(cancellationToken);

        var leaveBalances = await _context.LeaveBalances
            .Where(lb => lb.UserInfoId == request.UserId)
            .Select(lb => new
            {
                Type = lb.LeaveType,
                Total = lb.TotalAllowed,
                Used = lb.Used,
                Remaining = lb.Remaining
            })
            .ToListAsync(cancellationToken);

        return new MobileSummaryDto
        {
            WorkedDays = logs.Select(l => l.PunchTime.Date).Distinct().Count(),
            LateDays = 0, // Simplified logic for demo
            AbsentDays = 0, // Simplified logic
            LeaveBalances = leaveBalances
        };
    }

    public async Task<object> Handle(GetDepartmentUsersSummaryQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null || user.DepartmentId == null) return new List<object>();

        var now = DateTime.UtcNow;
        var targetMonth = request.Month ?? now.Month;
        var targetYear = request.Year ?? now.Year;

        var query = _context.UserInfos.Where(u => u.DepartmentId == user.DepartmentId);
        var total = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(u => u.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new
            {
                UserId = u.Id,
                Name = u.FirstName + " " + u.LastName,
                IsPresentToday = _context.AttendanceLogs.Any(l => l.UserInfoId == u.Id && l.PunchTime.Date == now.Date)
            })
            .ToListAsync(cancellationToken);

        return new { total, page = request.Page, pageSize = request.PageSize, data = users };
    }

    public async Task<object> Handle(GetDailyLogsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var targetMonth = request.Month ?? now.Month;
        var targetYear = request.Year ?? now.Year;

        var rangeStart = new DateTime(targetYear, targetMonth, 1, 0, 0, 0, DateTimeKind.Utc);
        var rangeEnd = rangeStart.AddMonths(1).AddDays(-1);

        var query = _context.AttendanceLogs
            .Where(l => l.UserInfoId == request.UserId && l.PunchTime >= rangeStart && l.PunchTime <= rangeEnd.AddDays(1).AddTicks(-1));
        
        var total = await query.CountAsync(cancellationToken);

        var logs = await query
            .OrderByDescending(l => l.PunchTime)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new
            {
                Date = l.PunchTime.Date,
                Time = l.PunchTime.ToString("HH:mm"),
                Type = l.PunchType.ToString(),
                Source = l.LogsType.ToString()
            })
            .ToListAsync(cancellationToken);

        return new { total, page = request.Page, pageSize = request.PageSize, data = logs };
    }

    public async Task<object> Handle(GetDepartmentColleaguesQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.UserInfos.FindAsync(new object[] { request.UserId }, cancellationToken);
        if (user == null || user.DepartmentId == null) return new List<object>();

        var query = _context.UserInfos.Where(u => u.DepartmentId == user.DepartmentId && u.Id != request.UserId);
        var total = await query.CountAsync(cancellationToken);

        var colleagues = await query
            .OrderBy(u => u.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new
            {
                Id = u.Id,
                Name = u.FirstName + " " + u.LastName,
                Email = u.Email,
                Photo = u.ProfilePictureUrl
            })
            .ToListAsync(cancellationToken);

        return new { total, page = request.Page, pageSize = request.PageSize, data = colleagues };
    }

    public async Task<object> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Notifications.Where(n => n.UserId == request.UserId);

        if (request.UnreadOnly)
            query = query.Where(n => !n.IsRead);

        var total = await query.CountAsync(cancellationToken);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(n => new
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead
            })
            .ToListAsync(cancellationToken);

        return new { total, page = request.Page, pageSize = request.PageSize, data = notifications };
    }

    public async Task<object> Handle(GetAllDevicesQuery request, CancellationToken cancellationToken)
    {
        // For mobile geo-fencing or ip tracking
        var query = _context.Devices.AsQueryable();
        var total = await query.CountAsync(cancellationToken);

        var devices = await query
            .OrderBy(d => d.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(d => new
            {
                Id = d.Id,
                Name = d.DeviceName,
                IpAddress = d.IpAddress,
                Port = d.Port,
                IsActive = d.IsActive
            })
            .ToListAsync(cancellationToken);

        return new { total, page = request.Page, pageSize = request.PageSize, data = devices };
    }
}
