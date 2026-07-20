using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;

namespace HR.Application.Attendance.ZKPython.Tickets.Commands;

public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, TicketOperationResult>
{
    private readonly ITicketRepository _ticketRepository;

    public CreateTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<TicketOperationResult> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            DateTime? dueDate = request.Request.Priority switch
            {
                Domain.Enums.TicketPriority.Critical => DateTime.UtcNow.AddHours(4),
                Domain.Enums.TicketPriority.High => DateTime.UtcNow.AddHours(24),
                Domain.Enums.TicketPriority.Medium => DateTime.UtcNow.AddDays(3),
                Domain.Enums.TicketPriority.Low => DateTime.UtcNow.AddDays(7),
                _ => null
            };

            var ticket = new Ticket
            {
                Title = request.Request.Title,
                Description = request.Request.Description,
                Type = request.Request.Type,
                Priority = request.Request.Priority,
                CreatedByUserId = request.CreatedByUserId,
                CreatedAt = DateTime.UtcNow,

                // SLA
                DueDate = dueDate,
                IsEscalated = false,

                // Shared workflow
                ManagerId = request.Request.ManagerId,
                Category = request.Request.Category,
                SubCategory = request.Request.SubCategory,

                // IT Specific
                ComponentName = request.Request.ComponentName,
                HardwareSoftware = request.Request.HardwareSoftware,

                // PKI Specific
                RequestType = request.Request.RequestType,
                TokenType = request.Request.TokenType,
                EndDate = request.Request.EndDate
            };

            await _ticketRepository.AddAsync(ticket, cancellationToken);

            return new TicketOperationResult
            {
                Success = true,
                Message = "ØªÙ… Ø¥Ù†Ø´Ø§Ø¡ Ø§Ù„ØªØ°ÙƒØ±Ø© Ø¨Ù†Ø¬Ø§Ø­.",
                TicketId = ticket.Id
            };
        }
        catch (Exception ex)
        {
            return new TicketOperationResult
            {
                Success = false,
                Message = "Ø­Ø¯Ø« Ø®Ø·Ø£ Ø£Ø«Ù†Ø§Ø¡ Ø¥Ù†Ø´Ø§Ø¡ Ø§Ù„ØªØ°ÙƒØ±Ø©.",
                ErrorDetail = ex.Message
            };
        }
    }
}
