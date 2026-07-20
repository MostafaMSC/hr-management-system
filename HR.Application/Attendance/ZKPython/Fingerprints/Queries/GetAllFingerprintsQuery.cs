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
/// Query Ù„Ù„Ø­ØµÙˆÙ„ Ø¹Ù„Ù‰ Ø¬Ù…ÙŠØ¹ Ø§Ù„Ø¨ØµÙ…Ø§Øª (Ù„Ù„ØªØ´Ø®ÙŠØµ)
/// </summary>
public record GetAllHRsQuery() : IRequest<List<HRDto>>;
