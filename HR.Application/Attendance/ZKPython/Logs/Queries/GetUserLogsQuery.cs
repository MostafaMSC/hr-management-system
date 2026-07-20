using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Logs.Queries;

public record GetUserLogsQuery(string UserId, string? DeviceIp) : IRequest<List<AttendanceLog>>;

public class GetUserLogsQueryHandler : IRequestHandler<GetUserLogsQuery, List<AttendanceLog>>
{
    private readonly IAttendanceLogRepository _repository;

    public GetUserLogsQueryHandler(IAttendanceLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AttendanceLog>> Handle(GetUserLogsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetLogsByUserIdAsync(request.UserId, request.DeviceIp);
    }
}
