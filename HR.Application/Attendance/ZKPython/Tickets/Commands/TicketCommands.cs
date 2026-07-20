using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;

namespace HR.Application.Attendance.ZKPython.Tickets.Commands;

public record CreateTicketCommand(int CreatedByUserId, CreateTicketRequest Request) : IRequest<TicketOperationResult>;
public record UpdateTicketStatusCommand(int TicketId, UpdateTicketStatusRequest Request, int UserId) : IRequest<TicketOperationResult>;
public record AssignTicketCommand(int TicketId, AssignTicketRequest Request, int AdminUserId) : IRequest<TicketOperationResult>;
public record AddTicketCommentCommand(int TicketId, AddTicketCommentRequest Request, int UserId) : IRequest<TicketOperationResult>;
