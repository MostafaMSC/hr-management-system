using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Application.Leaves.DTOs;
using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

using HR.Application.Common.Models;

namespace HR.Application.Leaves.Queries;

// MY LEAVE REQUESTS
public class GetEmployeeLeaveRequestsQuery : IRequest<PaginatedResult<LeaveRequestDto>>
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

public class GetEmployeeLeaveRequestsQueryHandler : IRequestHandler<GetEmployeeLeaveRequestsQuery, PaginatedResult<LeaveRequestDto>>
{
    private readonly IApplicationDbContext _context;
    public GetEmployeeLeaveRequestsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedResult<LeaveRequestDto>> Handle(GetEmployeeLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LeaveRequests
            .Include(l => l.UserInfo)
            .Where(l => l.UserInfoId == request.UserId);

        if (request.Status.HasValue)
            query = query.Where(l => l.Status == request.Status.Value);

        query = query.OrderByDescending(l => l.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            var skip = (request.PageNumber.Value - 1) * request.PageSize.Value;
            query = query.Skip(skip).Take(request.PageSize.Value);
        }

        var data = await query.Select(l => new LeaveRequestDto
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

        return new PaginatedResult<LeaveRequestDto>(data, totalCount, request.PageNumber ?? 1, request.PageSize ?? 10);
    }
}

// DEPARTMENT LEAVE REQUESTS (For Manager)
public class GetDepartmentLeaveRequestsQuery : IRequest<PaginatedResult<LeaveRequestDto>>
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

public class GetDepartmentLeaveRequestsQueryHandler : IRequestHandler<GetDepartmentLeaveRequestsQuery, PaginatedResult<LeaveRequestDto>>
{
    private readonly IApplicationDbContext _context;
    public GetDepartmentLeaveRequestsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedResult<LeaveRequestDto>> Handle(GetDepartmentLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LeaveRequests
            .Include(l => l.UserInfo)
            .Where(l => l.UserInfo.DirectManagerId == request.ManagerId);

        if (request.Status.HasValue)
            query = query.Where(l => l.Status == request.Status.Value);

        query = query.OrderByDescending(l => l.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            var skip = (request.PageNumber.Value - 1) * request.PageSize.Value;
            query = query.Skip(skip).Take(request.PageSize.Value);
        }

        var data = await query.Select(l => new LeaveRequestDto
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

        return new PaginatedResult<LeaveRequestDto>(data, totalCount, request.PageNumber ?? 1, request.PageSize ?? 10);
    }
}

// ALL LEAVE REQUESTS (For HR)
public class GetAllLeaveRequestsQuery : IRequest<PaginatedResult<LeaveRequestDto>>
{
    public LeaveStatus? Status { get; set; }
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }

    public GetAllLeaveRequestsQuery(LeaveStatus? status)
    {
        Status = status;
    }
}

public class GetAllLeaveRequestsQueryHandler : IRequestHandler<GetAllLeaveRequestsQuery, PaginatedResult<LeaveRequestDto>>
{
    private readonly IApplicationDbContext _context;
    public GetAllLeaveRequestsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedResult<LeaveRequestDto>> Handle(GetAllLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.LeaveRequests
            .Include(l => l.UserInfo)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(l => l.Status == request.Status.Value);

        query = query.OrderByDescending(l => l.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            var skip = (request.PageNumber.Value - 1) * request.PageSize.Value;
            query = query.Skip(skip).Take(request.PageSize.Value);
        }

        var data = await query.Select(l => new LeaveRequestDto
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

        return new PaginatedResult<LeaveRequestDto>(data, totalCount, request.PageNumber ?? 1, request.PageSize ?? 10);
    }
}
