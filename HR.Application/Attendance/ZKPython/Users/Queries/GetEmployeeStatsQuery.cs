using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Common.Interfaces;
using HR.Domain.Enums;
using HR.Domain.Entities;

namespace HR.Application.Attendance.ZKPython.Users.Queries;

public record GetEmployeeStatsQuery(DateTime? Date = null) : IRequest<EmployeeStatsDto>;

public class GetEmployeeStatsQueryHandler : IRequestHandler<GetEmployeeStatsQuery, EmployeeStatsDto>
{
    private readonly IAttendanceLogRepository _logRepository;
    private readonly ILeaveRepository _leaveRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly ISettingsRepository _settingsRepository;

    public GetEmployeeStatsQueryHandler(
        IAttendanceLogRepository logRepository,
        ILeaveRepository leaveRepository,
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        ISettingsRepository settingsRepository)
    {
        _logRepository = logRepository;
        _leaveRepository = leaveRepository;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _settingsRepository = settingsRepository;
    }

    public async Task<EmployeeStatsDto> Handle(GetEmployeeStatsQuery request, CancellationToken cancellationToken)
    {
        return null;
    }
}
