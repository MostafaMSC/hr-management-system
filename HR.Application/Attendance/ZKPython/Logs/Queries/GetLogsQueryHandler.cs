using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Attendance.ZKPython.Logs.DTOs;
using HR.Application.Common.Interfaces;

namespace HR.Application.Attendance.ZKPython.Logs.Queries;

public class GetLogsQueryHandler : IRequestHandler<GetLogsQuery, LogsResult>
{
    private readonly IAttendanceLogRepository _logRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetLogsQueryHandler(
        IAttendanceLogRepository logRepository,
        ICurrentUserService currentUserService)
    {
        _logRepository = logRepository;
        _currentUserService = currentUserService;
    }

    public async Task<LogsResult> Handle(GetLogsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 100 : request.PageSize;

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

        var (logs, total) = await _logRepository.GetPagedLogsAsync(
            page,
            pageSize,
            request.DeviceIp,
            filterUserId,
            filterDeptId,
            request.StartDate,
            request.EndDate,
            request.EmployeeId,
            cancellationToken);

        return new LogsResult
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Data = logs
        };
    }
}
