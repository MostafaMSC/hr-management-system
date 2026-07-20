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
using HR.Domain.Enums;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;

namespace HR.Application.Attendance.ZKPython.Tickets.Commands;

public class UpdateTicketStatusCommandHandler : IRequestHandler<UpdateTicketStatusCommand, TicketOperationResult>
{
    private readonly ITicketRepository _ticketRepository;

    public UpdateTicketStatusCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<TicketOperationResult> Handle(UpdateTicketStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
            if (ticket == null)
            {
                return new TicketOperationResult { Success = false, Message = "Ø§Ù„ØªØ°ÙƒØ±Ø© ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯Ø©." };
            }

            if (ticket.Status == TicketStatus.WaitingManagerApproval && request.UserId != ticket.ManagerId)
            {
                // Only the assigned Manager can change this status (approve/reject).
                // Or maybe an IT Admin. For now, strict check on ManagerId if it exists.
                if (ticket.ManagerId.HasValue && ticket.ManagerId.Value != request.UserId)
                {
                    return new TicketOperationResult { Success = false, Message = "Only the assigned Manager can approve this ticket." };
                }
            }

            // If updating from WaitingUserVerification to Closed, typically only the CreatedByUserId can do it
            if (ticket.Status == TicketStatus.WaitingUserVerification && request.Request.Status == TicketStatus.Closed)
            {
                if (ticket.CreatedByUserId != request.UserId)
                {
                    return new TicketOperationResult { Success = false, Message = "Only the ticket creator can verify and close the ticket." };
                }
            }

            ticket.Status = request.Request.Status;
            ticket.UpdatedAt = DateTime.UtcNow;

            if (request.Request.Status == TicketStatus.Resolved || request.Request.Status == TicketStatus.Closed)
            {
                ticket.ResolvedAt = DateTime.UtcNow;
            }

            await _ticketRepository.UpdateAsync(ticket, cancellationToken);

            return new TicketOperationResult
            {
                Success = true,
                Message = "ØªÙ… ØªØ­Ø¯ÙŠØ« Ø­Ø§Ù„Ø© Ø§Ù„ØªØ°ÙƒØ±Ø© Ø¨Ù†Ø¬Ø§Ø­.",
                TicketId = ticket.Id
            };
        }
        catch (Exception ex)
        {
            return new TicketOperationResult { Success = false, Message = "Ø­Ø¯Ø« Ø®Ø·Ø£ Ø£Ø«Ù†Ø§Ø¡ Ø§Ù„ØªØ­Ø¯ÙŠØ«.", ErrorDetail = ex.Message };
        }
    }
}

public class AssignTicketCommandHandler : IRequestHandler<AssignTicketCommand, TicketOperationResult>
{
    private readonly ITicketRepository _ticketRepository;

    public AssignTicketCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<TicketOperationResult> Handle(AssignTicketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
            if (ticket == null) return new TicketOperationResult { Success = false, Message = "Ø§Ù„ØªØ°ÙƒØ±Ø© ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯Ø©." };

            ticket.AssignedToUserId = request.Request.AssignedToUserId;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _ticketRepository.UpdateAsync(ticket, cancellationToken);

            return new TicketOperationResult { Success = true, Message = "ØªÙ… ØªØ¹ÙŠÙŠÙ† Ø§Ù„ØªØ°ÙƒØ±Ø© Ø¨Ù†Ø¬Ø§Ø­.", TicketId = ticket.Id };
        }
        catch (Exception ex)
        {
            return new TicketOperationResult { Success = false, Message = "Ø®Ø·Ø£ Ø£Ø«Ù†Ø§Ø¡ Ø§Ù„ØªØ¹ÙŠÙŠÙ†.", ErrorDetail = ex.Message };
        }
    }
}

public class AddTicketCommentCommandHandler : IRequestHandler<AddTicketCommentCommand, TicketOperationResult>
{
    private readonly ITicketRepository _ticketRepository;

    public AddTicketCommentCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<TicketOperationResult> Handle(AddTicketCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
            if (ticket == null) return new TicketOperationResult { Success = false, Message = "Ø§Ù„ØªØ°ÙƒØ±Ø© ØºÙŠØ± Ù…ÙˆØ¬ÙˆØ¯Ø©." };

            var comment = new HR.Domain.Entities.TicketComment
            {
                TicketId = request.TicketId,
                UserId = request.UserId,
                CommentText = request.Request.CommentText,
                CreatedAt = DateTime.UtcNow
            };

            await _ticketRepository.AddCommentAsync(comment, cancellationToken);

            return new TicketOperationResult { Success = true, Message = "ØªÙ…Øª Ø¥Ø¶Ø§ÙØ© Ø§Ù„ØªØ¹Ù„ÙŠÙ‚.", TicketId = ticket.Id };
        }
        catch (Exception ex)
        {
            return new TicketOperationResult { Success = false, Message = "Ø®Ø·Ø£ Ø£Ø«Ù†Ø§Ø¡ Ø¥Ø¶Ø§ÙØ© Ø§Ù„ØªØ¹Ù„ÙŠÙ‚.", ErrorDetail = ex.Message };
        }
    }
}
