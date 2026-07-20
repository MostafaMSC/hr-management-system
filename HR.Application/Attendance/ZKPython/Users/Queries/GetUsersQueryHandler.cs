using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Domain.Entities;
using HR.Application.Common.Interfaces;

namespace HR.Application.Attendance.ZKPython.Users.Queries;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserInfo>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUsersQueryHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<UserInfo>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var role = _currentUserService.Role;
        var userId = _currentUserService.UserId;
        var deptId = _currentUserService.DepartmentId;

        if (role == Domain.Enums.UserType.Administrator)
        {
            return await _userRepository.GetUsersAsync(request.DeviceIp, cancellationToken);
        }
        else if (role == Domain.Enums.UserType.Manager)
        {
            if (deptId.HasValue)
            {
                // Manager sees users in their department
                // If requesting specific DeviceIp, we might need to filter further (or ignore DeviceIp filter for manager security view)
                // Existing implementation has GetUsersAsync(deviceIp)
                // We should likely prioritize Department filter.
                return await _userRepository.GetUsersByDepartmentIdAsync(deptId.Value, cancellationToken);
            }
            // If no department assigned, maybe return empty or self?
            return new List<UserInfo>();
        }
        else // User
        {
            if (userId.HasValue)
            {
                 var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
                 return user != null ? new List<UserInfo> { user } : new List<UserInfo>();
            }
            return new List<UserInfo>();
        }
    }
}
