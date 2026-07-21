using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Domain.Entities;
using HR.Application.Common.Interfaces;

using HR.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Attendance.ZKPython.Users.Queries;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedResult<UserInfo>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUsersQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<UserInfo>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var role = _currentUserService.Role;
        var userId = _currentUserService.UserId;
        var deptId = _currentUserService.DepartmentId;

        var query = _context.UserInfos.AsQueryable();

        if (role == Domain.Enums.UserType.Manager && deptId.HasValue)
        {
            query = query.Where(u => u.DepartmentId == deptId.Value);
        }
        else if (role != Domain.Enums.UserType.Administrator && role != Domain.Enums.UserType.Manager)
        {
            if (userId.HasValue)
            {
                query = query.Where(u => u.Id == userId.Value);
            }
            else
            {
                return new PaginatedResult<UserInfo> { Page = request.Page, PageSize = request.PageSize };
            }
        }

        if (!string.IsNullOrEmpty(request.DeviceIp))
        {
            query = query.Where(u => u.DeviceIp == request.DeviceIp);
        }

        var total = await query.CountAsync(cancellationToken);
        
        var data = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<UserInfo>
        {
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize,
            Data = data
        };
    }
}
