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

public class SearchLogsQueryHandler : IRequestHandler<SearchLogsQuery, PaginatedResult<AttendanceLog>>
{
    private readonly IAttendanceLogRepository _repository;

    public SearchLogsQueryHandler(IAttendanceLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedResult<AttendanceLog>> Handle(SearchLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _repository.SearchByNameAsync(request.Name);
        var totalCount = logs.Count;
        var data = logs.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList();
        return new PaginatedResult<AttendanceLog>(data, totalCount, request.PageNumber, request.PageSize);
    }
}
