using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using HR.Domain.Entities;
using MediatR;

using HR.Application.Common.Models;

namespace HR.Application.Attendance.ZKPython.Logs.Queries;

public record SearchLogsQuery(string Name, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<AttendanceLog>>;
