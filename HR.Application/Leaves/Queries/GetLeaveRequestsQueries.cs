using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Application.Leaves.DTOs;
using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Leaves.Queries;

// MY LEAVE REQUESTS
public class GetEmployeeLeaveRequestsQuery : IRequest<List<LeaveRequestDto>>
{
    public int UserId { get; set; }
    public LeaveStatus? Status { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }

    public GetEmployeeLeaveRequestsQuery(int userId, LeaveStatus? status)
    {
        UserId = userId;
        Status = status;
    }
}

public class GetEmployeeLeaveRequestsQueryHandler : IRequestHandler<GetEmployeeLeaveRequestsQuery, List<LeaveRequestDto>>
{
    private readonly IApplicationDbContext _context;
    public GetEmployeeLeaveRequestsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<LeaveRequestDto>> Handle(GetEmployeeLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LeaveRequests
            .Include(l => l.UserInfo)
            .Where(l => l.UserInfoId == request.UserId);

        if (request.Status.HasValue)
            query = query.Where(l => l.Status == request.Status.Value);

        query = query.OrderByDescending(l => l.CreatedAt);

        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            var skip = (request.PageNumber.Value - 1) * request.PageSize.Value;
            query = query.Skip(skip).Take(request.PageSize.Value);
        }

        return await query.Select(l => new LeaveRequestDto
        {
            Id = l.Id,
            EmployeeId = l.UserInfoId,
            EmployeeName = $"{l.UserInfo.FirstName} {l.UserInfo.LastName}",
            LeaveType = l.LeaveType.ToString(),
            RequestedShiftId = l.RequestedShiftId,
            LeaveReason = l.LeaveReason,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            LeaveDate = l.LeaveDate,
            StartTime = l.StartTime.HasValue ? l.StartTime.Value.ToString(@"hh\:mm") : null,
            EndTime = l.EndTime.HasValue ? l.EndTime.Value.ToString(@"hh\:mm") : null,
            Reason = l.Reason,
            Status = l.Status.ToString(),
            ManagerComment = l.ManagerComment,
            HrComment = l.HrComment,
            AttachmentUrl = l.AttachmentUrl,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt
        }).ToListAsync(cancellationToken);
    }
}

// DEPARTMENT LEAVE REQUESTS (For Manager)
public class GetDepartmentLeaveRequestsQuery : IRequest<List<LeaveRequestDto>>
{
    public int ManagerId { get; set; }
    public LeaveStatus? Status { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }

    public GetDepartmentLeaveRequestsQuery(int managerId, LeaveStatus? status)
    {
        ManagerId = managerId;
        Status = status;
    }
}

public class GetDepartmentLeaveRequestsQueryHandler : IRequestHandler<GetDepartmentLeaveRequestsQuery, List<LeaveRequestDto>>
{
    private readonly IApplicationDbContext _context;
    public GetDepartmentLeaveRequestsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<LeaveRequestDto>> Handle(GetDepartmentLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LeaveRequests
            .Include(l => l.UserInfo)
            .Where(l => l.UserInfo.DirectManagerId == request.ManagerId);

        if (request.Status.HasValue)
            query = query.Where(l => l.Status == request.Status.Value);

        query = query.OrderByDescending(l => l.CreatedAt);

        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            var skip = (request.PageNumber.Value - 1) * request.PageSize.Value;
            query = query.Skip(skip).Take(request.PageSize.Value);
        }

        return await query.Select(l => new LeaveRequestDto
        {
            Id = l.Id,
            EmployeeId = l.UserInfoId,
            EmployeeName = $"{l.UserInfo.FirstName} {l.UserInfo.LastName}",
            LeaveType = l.LeaveType.ToString(),
            RequestedShiftId = l.RequestedShiftId,
            LeaveReason = l.LeaveReason,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            LeaveDate = l.LeaveDate,
            StartTime = l.StartTime.HasValue ? l.StartTime.Value.ToString(@"hh\:mm") : null,
            EndTime = l.EndTime.HasValue ? l.EndTime.Value.ToString(@"hh\:mm") : null,
            Reason = l.Reason,
            Status = l.Status.ToString(),
            ManagerComment = l.ManagerComment,
            HrComment = l.HrComment,
            AttachmentUrl = l.AttachmentUrl,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt
        }).ToListAsync(cancellationToken);
    }
}

// ALL LEAVE REQUESTS (For HR)
public class GetAllLeaveRequestsQuery : IRequest<List<LeaveRequestDto>>
{
    public LeaveStatus? Status { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }

    public GetAllLeaveRequestsQuery(LeaveStatus? status)
    {
        Status = status;
    }
}

public class GetAllLeaveRequestsQueryHandler : IRequestHandler<GetAllLeaveRequestsQuery, List<LeaveRequestDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAllLeaveRequestsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<LeaveRequestDto>> Handle(GetAllLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LeaveRequests
            .Include(l => l.UserInfo)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(l => l.Status == request.Status.Value);

        query = query.OrderByDescending(l => l.CreatedAt);

        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            var skip = (request.PageNumber.Value - 1) * request.PageSize.Value;
            query = query.Skip(skip).Take(request.PageSize.Value);
        }

        return await query.Select(l => new LeaveRequestDto
        {
            Id = l.Id,
            EmployeeId = l.UserInfoId,
            EmployeeName = $"{l.UserInfo.FirstName} {l.UserInfo.LastName}",
            LeaveType = l.LeaveType.ToString(),
            RequestedShiftId = l.RequestedShiftId,
            LeaveReason = l.LeaveReason,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            LeaveDate = l.LeaveDate,
            StartTime = l.StartTime.HasValue ? l.StartTime.Value.ToString(@"hh\:mm") : null,
            EndTime = l.EndTime.HasValue ? l.EndTime.Value.ToString(@"hh\:mm") : null,
            Reason = l.Reason,
            Status = l.Status.ToString(),
            ManagerComment = l.ManagerComment,
            HrComment = l.HrComment,
            AttachmentUrl = l.AttachmentUrl,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt
        }).ToListAsync(cancellationToken);
    }
}
