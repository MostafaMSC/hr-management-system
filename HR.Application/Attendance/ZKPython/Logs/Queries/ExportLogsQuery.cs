using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Logs.Queries;

public record ExportLogsQuery(string? DeviceIp, DateTime? DateFrom = null, DateTime? DateTo = null) : IRequest<IAsyncEnumerable<AttendanceLog>>;

public class ExportLogsQueryHandler : IRequestHandler<ExportLogsQuery, IAsyncEnumerable<AttendanceLog>>
{
    private readonly IAttendanceLogRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public ExportLogsQueryHandler(IAttendanceLogRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public Task<IAsyncEnumerable<AttendanceLog>> Handle(ExportLogsQuery request, CancellationToken cancellationToken)
    {
        var role = _currentUserService.Role;
        int? filterUserId = null;
        int? filterDeptId = null;

        if (role == Domain.Enums.UserType.Manager)
        {
            filterDeptId = _currentUserService.DepartmentId;
        }
        if (role == Domain.Enums.UserType.Employee)
        {
            filterUserId = _currentUserService.UserId;
        }

        return Task.FromResult(_repository.GetAllLogsStreamAsync(request.DeviceIp, filterUserId, filterDeptId, request.DateFrom, request.DateTo));
    }
}
