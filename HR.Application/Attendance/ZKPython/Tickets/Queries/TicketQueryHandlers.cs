using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;

namespace HR.Application.Attendance.ZKPython.Tickets.Queries;

public class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, List<TicketDto>>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketsQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<List<TicketDto>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        var rawTickets = await _ticketRepository.GetAllAsync(request.Type, request.Status, cancellationToken);

        var tickets = rawTickets.Select(t => new TicketDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Type = t.Type.ToString(),
            Status = t.Status.ToString(),
            Priority = t.Priority.ToString(),
            CreatedByUserId = t.CreatedByUserId,
            CreatedByUserName = t.CreatedBy != null ? t.CreatedBy.Username : string.Empty,
            AssignedToUserId = t.AssignedToUserId,
            AssignedToUserName = t.AssignedTo != null ? t.AssignedTo.Username : null,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            ResolvedAt = t.ResolvedAt,

            // Workflow & SLA
            ManagerId = t.ManagerId,
            ManagerName = t.Manager != null ? t.Manager.Username : null,
            DueDate = t.DueDate,
            IsEscalated = t.IsEscalated,

            // Dynamic fields
            Category = t.Category,
            SubCategory = t.SubCategory,
            ComponentName = t.ComponentName,
            HardwareSoftware = t.HardwareSoftware,
            RequestType = t.RequestType,
            TokenType = t.TokenType,
            EndDate = t.EndDate
        }).ToList();

        return tickets;
    }
}

public class GetTicketByIdQueryHandler : IRequestHandler<GetTicketByIdQuery, TicketDetailsDto?>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketByIdQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<TicketDetailsDto?> Handle(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket == null) return null;

        var dto = new TicketDetailsDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Type = ticket.Type.ToString(),
            Status = ticket.Status.ToString(),
            Priority = ticket.Priority.ToString(),
            CreatedByUserId = ticket.CreatedByUserId,
            CreatedByUserName = ticket.CreatedBy != null ? ticket.CreatedBy.Username : string.Empty,
            AssignedToUserId = ticket.AssignedToUserId,
            AssignedToUserName = ticket.AssignedTo != null ? ticket.AssignedTo.Username : null,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ResolvedAt = ticket.ResolvedAt,

            // Workflow & SLA
            ManagerId = ticket.ManagerId,
            ManagerName = ticket.Manager != null ? ticket.Manager.Username : null,
            DueDate = ticket.DueDate,
            IsEscalated = ticket.IsEscalated,

            // Dynamic fields
            Category = ticket.Category,
            SubCategory = ticket.SubCategory,
            ComponentName = ticket.ComponentName,
            HardwareSoftware = ticket.HardwareSoftware,
            RequestType = ticket.RequestType,
            TokenType = ticket.TokenType,
            EndDate = ticket.EndDate,

            Comments = ticket.Comments.Select(c => new TicketCommentDto
            {
                Id = c.Id,
                TicketId = c.TicketId,
                UserId = c.UserId,
                UserName = c.User != null ? c.User.Username : string.Empty,
                CommentText = c.CommentText,
                CreatedAt = c.CreatedAt
            }).ToList(),
            Attachments = ticket.Attachments.Select(a => new TicketAttachmentDto
            {
                Id = a.Id,
                TicketId = a.TicketId,
                FileName = a.FileName,
                FilePath = a.FilePath,
                UploadedAt = a.UploadedAt
            }).ToList()
        };

        return dto;
    }
}
