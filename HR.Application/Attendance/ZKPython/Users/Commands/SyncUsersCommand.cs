using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Attendance.ZKPython.Users.DTOs;

namespace HR.Application.Attendance.ZKPython.Users.Commands;

public record SyncUsersCommand(string? DeviceIp) : IRequest<SyncUsersResult>;
