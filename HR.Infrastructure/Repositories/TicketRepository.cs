using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
namespace HR.Infrastructure.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        public Task<Ticket> AddAsync(Ticket ticket, CancellationToken cancellationToken = default) => Task.FromResult(ticket);
        public Task<Ticket?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<Ticket?>(null);
        public Task<List<Ticket>> GetAllAsync(TicketType? type, TicketStatus? status, CancellationToken cancellationToken = default) => Task.FromResult(new List<Ticket>());
        public Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<TicketComment> AddCommentAsync(TicketComment comment, CancellationToken cancellationToken = default) => Task.FromResult(comment);
        public Task<TicketAttachment> AddAttachmentAsync(TicketAttachment attachment, CancellationToken cancellationToken = default) => Task.FromResult(attachment);
    }
}
