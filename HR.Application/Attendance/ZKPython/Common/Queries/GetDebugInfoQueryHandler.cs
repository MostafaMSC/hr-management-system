using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HR.Application.Attendance.ZKPython.Common.Queries;

public class GetDebugInfoQueryHandler : IRequestHandler<GetDebugInfoQuery, DebugInfoDto>
{
    private readonly IAttendanceLogRepository _repository;
    private readonly IConfiguration _configuration;

    public GetDebugInfoQueryHandler(IAttendanceLogRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public async Task<DebugInfoDto> Handle(GetDebugInfoQuery request, CancellationToken cancellationToken)
    {
        var offset = _configuration.GetValue<double>("TimezoneOffset");

        // Get all logs for user to analyze
        var allUserLogs = await _repository.GetLogsByUserIdAsync(request.UserId);
        var recentLogs = allUserLogs.Take(10).Select(l => new LogDebugInfo
        {
            Time = l.PunchTime,
            TimeKind = l.PunchTime.Kind.ToString(),
            TimeFormatted = l.PunchTime.ToString("yyyy-MM-dd HH:mm:ss"),
            CheckStatus = l.CheckStatus
        }).ToList();

        var lastLog = allUserLogs.FirstOrDefault();

        return new DebugInfoDto
        {
            TimezoneOffset = offset,
            RawTime = lastLog?.Time,
            RawTimeKind = lastLog?.Time.Kind.ToString(),
            RawTimeFormatted = lastLog?.Time.ToString("yyyy-MM-dd HH:mm:ss"),
            RawTimeIso = lastLog?.Time.ToString("yyyy-MM-ddTHH:mm:ss"),
            ServerTime = DateTime.Now,
            ServerTimeFormatted = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            TotalLogsForUser = allUserLogs.Count,
            RecentLogs = recentLogs
        };
    }
}
