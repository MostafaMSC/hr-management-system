using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using MediatR;

using HR.Application.Common.Models;

namespace HR.Application.Attendance.ZKPython.Logs.Queries;

public record GetTodayLogsQuery(string? DeviceIp, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<AttendanceLog>>;

public class GetTodayLogsQueryHandler : IRequestHandler<GetTodayLogsQuery, PaginatedResult<AttendanceLog>>
{
    private readonly IAttendanceLogRepository _repository;

    public GetTodayLogsQueryHandler(IAttendanceLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResult<AttendanceLog>> Handle(GetTodayLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _repository.GetTodayLogsAsync(request.DeviceIp);
        var totalCount = logs.Count;
        var data = logs.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList();
        return new PaginatedResult<AttendanceLog>(data, totalCount, request.PageNumber, request.PageSize);
    }
}
