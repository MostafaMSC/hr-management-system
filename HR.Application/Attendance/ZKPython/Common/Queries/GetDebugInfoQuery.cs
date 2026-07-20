using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using System;

namespace HR.Application.Attendance.ZKPython.Common.Queries;

public record GetDebugInfoQuery(string UserId) : IRequest<DebugInfoDto>;

public class DebugInfoDto
{
    public double TimezoneOffset { get; set; }
    public DateTime? RawTime { get; set; }
    public string? RawTimeKind { get; set; }
    public string? RawTimeFormatted { get; set; }
    public string? RawTimeIso { get; set; }
    public DateTime ServerTime { get; set; }
    public string ServerTimeFormatted { get; set; } = string.Empty;
    public int TotalLogsForUser { get; set; }
    public List<LogDebugInfo>? RecentLogs { get; set; }
}

public class LogDebugInfo
{
    public DateTime Time { get; set; }
    public string TimeKind { get; set; } = string.Empty;
    public string TimeFormatted { get; set; } = string.Empty;
    public string? CheckStatus { get; set; }
}
