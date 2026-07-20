using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Attendance.ZKPython.DTOs;

namespace HR.Application.Attendance.ZKPython.UserDevices.Commands;

/// <summary>
/// Ø£Ù…Ø± Ù…Ø²Ø§Ù…Ù†Ø© Ø§Ù„Ø¨ØµÙ…Ø§Øª Ù…Ù† Ø£Ø¬Ù‡Ø²Ø© ZKTeco
/// </summary>
public record SyncHRsCommand(string? DeviceIp) : IRequest<SyncHRsResult>;
