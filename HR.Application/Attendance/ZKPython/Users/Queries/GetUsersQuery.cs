using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Domain.Entities;

namespace HR.Application.Attendance.ZKPython.Users.Queries;

public record GetUsersQuery(string? DeviceIp = null) : IRequest<List<UserInfo>>;
