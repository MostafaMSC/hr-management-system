using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Common.Queries;

public record GetRolesQuery() : IRequest<List<string?>>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<string?>>
{
    private readonly IAttendanceLogRepository _repository;

    public GetRolesQueryHandler(IAttendanceLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<string?>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetRolesAsync();
    }
}
