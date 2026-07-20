using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HR.Domain.Entities;
using HR.Domain.Enums;

namespace HR.Application.Common.Interfaces;

public interface ITicketRepository
{
    Task<Ticket> AddAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Ticket>> GetAllAsync(TicketType? type, TicketStatus? status, CancellationToken cancellationToken = default);
    Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default);

    Task<TicketComment> AddCommentAsync(TicketComment comment, CancellationToken cancellationToken = default);
    Task<TicketAttachment> AddAttachmentAsync(TicketAttachment attachment, CancellationToken cancellationToken = default);
}
