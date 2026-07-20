using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Logs.Queries;

public record GetLogsCountQuery(string? DeviceIp) : IRequest<int>;

public class GetLogsCountQueryHandler : IRequestHandler<GetLogsCountQuery, int>
{
    private readonly IAttendanceLogRepository _repository;

    public GetLogsCountQueryHandler(IAttendanceLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(GetLogsCountQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetLogsCountAsync(request.DeviceIp);
    }
}
