using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Attendance.ZKPython.DTOs;

namespace HR.Application.Attendance.ZKPython.UserDevices.Queries;

/// <summary>
/// Ø§Ø³ØªØ¹Ù„Ø§Ù… Ù„Ù„Ø­ØµÙˆÙ„ Ø¹Ù„Ù‰ Ø¨ØµÙ…Ø§Øª Ù…Ø³ØªØ®Ø¯Ù… Ù…Ø¹ÙŠÙ†
/// </summary>
public record GetUserHRsQuery(int UserId) : IRequest<List<HRDto>>;
