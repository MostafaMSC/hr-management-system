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

public record GetTodayLogsQuery(string? DeviceIp) : IRequest<List<AttendanceLog>>;

public class GetTodayLogsQueryHandler : IRequestHandler<GetTodayLogsQuery, List<AttendanceLog>>
{
    private readonly IAttendanceLogRepository _repository;

    public GetTodayLogsQueryHandler(IAttendanceLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AttendanceLog>> Handle(GetTodayLogsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetTodayLogsAsync(request.DeviceIp);
    }
}
