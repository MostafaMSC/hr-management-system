using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Users.Queries;

public record GetUsersCountByDepartmentQuery(string? DeviceIp) : IRequest<List<UserCountByDepartmentDto>>;

public class GetUsersCountByDepartmentQueryHandler : IRequestHandler<GetUsersCountByDepartmentQuery, List<UserCountByDepartmentDto>>
{
    private readonly IUserRepository _repository;

    public GetUsersCountByDepartmentQueryHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<UserCountByDepartmentDto>> Handle(GetUsersCountByDepartmentQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetCountByDepartmentAsync(request.DeviceIp, cancellationToken);
    }
}
