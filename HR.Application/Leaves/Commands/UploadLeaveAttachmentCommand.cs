using System;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Leaves.Commands;

public record UploadLeaveAttachmentCommand(int LeaveId, string AttachmentUrl) : IRequest<bool>;

public class UploadLeaveAttachmentCommandHandler : IRequestHandler<UploadLeaveAttachmentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UploadLeaveAttachmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UploadLeaveAttachmentCommand request, CancellationToken cancellationToken)
    {
        var leave = await _context.LeaveRequests.FirstOrDefaultAsync(l => l.Id == request.LeaveId, cancellationToken);

        if (leave == null)
            throw new NotFoundException($"Leave request {request.LeaveId} not found.");

        leave.AttachmentUrl = request.AttachmentUrl;
        leave.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
