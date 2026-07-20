using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Reports.Queries;

public record GetLateQuery(string Time, string? DeviceIp) : IRequest<List<AttendanceLog>>;

public class GetLateQueryHandler : IRequestHandler<GetLateQuery, List<AttendanceLog>>
{
    private readonly IAttendanceLogRepository _repository;

    public GetLateQueryHandler(IAttendanceLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AttendanceLog>> Handle(GetLateQuery request, CancellationToken cancellationToken)
    {
        if (!TimeSpan.TryParse(request.Time, out TimeSpan lateTime))
            lateTime = new TimeSpan(8, 30, 0);

        return await _repository.GetLateLogsAsync(lateTime, DateTime.Today, request.DeviceIp);
    }
}
