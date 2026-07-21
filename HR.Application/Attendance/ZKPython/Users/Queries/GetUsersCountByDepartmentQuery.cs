using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Attendance.ZKPython.Users.Queries;

public record GetUsersCountByDepartmentQuery(string? DeviceIp) : IRequest<List<UserCountByDepartmentDto>>;

public class GetUsersCountByDepartmentQueryHandler : IRequestHandler<GetUsersCountByDepartmentQuery, List<UserCountByDepartmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersCountByDepartmentQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserCountByDepartmentDto>> Handle(GetUsersCountByDepartmentQuery request, CancellationToken cancellationToken)
    {
        var query = _context.UserInfos.AsQueryable();
        
        if (!string.IsNullOrEmpty(request.DeviceIp))
        {
            query = query.Where(u => u.DeviceIp == request.DeviceIp);
        }

        var grouped = await query
            .GroupBy(u => u.Department != null ? u.Department.Name : "Unassigned")
            .Select(g => new UserCountByDepartmentDto { Department = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return grouped;
    }
}
