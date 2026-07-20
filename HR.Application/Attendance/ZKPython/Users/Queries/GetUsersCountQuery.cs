using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Users.Queries;

public record GetUsersCountQuery(string? DeviceIp) : IRequest<int>;

public class GetUsersCountQueryHandler : IRequestHandler<GetUsersCountQuery, int>
{
    private readonly IUserRepository _repository;

    public GetUsersCountQueryHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(GetUsersCountQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetCountAsync(request.DeviceIp, cancellationToken);
    }
}
