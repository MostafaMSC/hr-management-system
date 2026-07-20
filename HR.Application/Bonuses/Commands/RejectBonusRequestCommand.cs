using System;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Domain.Common;
using HR.Domain.Entities;
using HR.Domain.Enums;
using MediatR;

namespace HR.Application.Bonuses.Commands;

public record RejectBonusRequestCommand(int BonusRequestId, int HrUserId, string RejectionReason) : IRequest<Result<bool>>;

public class RejectBonusRequestCommandHandler : IRequestHandler<RejectBonusRequestCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public RejectBonusRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(RejectBonusRequestCommand request, CancellationToken cancellationToken)
    {
        var bonusRequest = await _context.BonusRequests.FindAsync(new object[] { request.BonusRequestId }, cancellationToken);

        if (bonusRequest == null) return Result<bool>.Failure("Bonus request not found.");
        if (bonusRequest.Status != BonusStatus.Pending) return Result<bool>.Failure("Bonus request is not pending.");

        bonusRequest.Status = BonusStatus.Rejected;
        bonusRequest.ProcessedByHrId = request.HrUserId;
        bonusRequest.ProcessedAt = DateTime.UtcNow;

        // Add Notification for Manager
        _context.Notifications.Add(new Notification
        {
            UserId = bonusRequest.RequestingManagerId,
            Title = "Bonus Request Rejected",
            Message = $"Your bonus request for employee ID {bonusRequest.TargetUserId} was rejected. Reason: {request.RejectionReason}",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        // Notification for Employee
        _context.Notifications.Add(new Notification
        {
            UserId = bonusRequest.TargetUserId,
            Title = "Bonus Request Rejected",
            Message = $"A bonus request for you was rejected by HR. Reason: {request.RejectionReason}",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
