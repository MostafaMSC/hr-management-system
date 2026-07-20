using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using System.Collections.Generic;
using MediatR;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Domain.Enums;

namespace HR.Application.Attendance.ZKPython.Tickets.Queries;

public record GetTicketsQuery(TicketType? Type, TicketStatus? Status) : IRequest<List<TicketDto>>;
public record GetTicketByIdQuery(int TicketId) : IRequest<TicketDetailsDto?>;
