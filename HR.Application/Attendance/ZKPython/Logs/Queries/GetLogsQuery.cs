using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Attendance.ZKPython.Logs.DTOs;

namespace HR.Application.Attendance.ZKPython.Logs.Queries;

public record GetLogsQuery(int Page = 1, int PageSize = 100, string? DeviceIp = null, DateTime? StartDate = null, DateTime? EndDate = null, string? EmployeeId = null) : IRequest<LogsResult>;
