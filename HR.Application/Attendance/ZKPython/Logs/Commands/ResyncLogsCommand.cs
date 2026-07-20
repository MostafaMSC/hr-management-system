using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Logs.Commands;

public record ResyncLogsCommand(string DeviceIp) : IRequest<ResyncLogsResult>;

public class ResyncLogsResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DeletedCount { get; set; }
    public int AddedCount { get; set; }
    public string? ErrorDetail { get; set; }
}
