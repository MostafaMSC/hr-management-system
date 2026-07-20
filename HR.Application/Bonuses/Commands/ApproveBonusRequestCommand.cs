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

public record ApproveBonusRequestCommand(int BonusRequestId, int HrUserId) : IRequest<Result<bool>>;

public class ApproveBonusRequestCommandHandler : IRequestHandler<ApproveBonusRequestCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public ApproveBonusRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(ApproveBonusRequestCommand request, CancellationToken cancellationToken)
    {
        var bonusRequest = await _context.BonusRequests.FindAsync(new object[] { request.BonusRequestId }, cancellationToken);

        if (bonusRequest == null) return Result<bool>.Failure("Bonus request not found.");
        if (bonusRequest.Status != BonusStatus.Pending) return Result<bool>.Failure("Bonus request is not pending.");

        bonusRequest.Status = BonusStatus.Approved;
        bonusRequest.ProcessedByHrId = request.HrUserId;
        bonusRequest.ProcessedAt = DateTime.UtcNow;

        // Add Notification for Manager
        _context.Notifications.Add(new Notification
        {
            UserId = bonusRequest.RequestingManagerId,
            Title = "Bonus Request Approved",
            Message = $"Your bonus request for employee ID {bonusRequest.TargetUserId} has been approved.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        // Add Notification for Employee
        _context.Notifications.Add(new Notification
        {
            UserId = bonusRequest.TargetUserId,
            Title = "Bonus Awarded",
            Message = $"You have been awarded a bonus for {bonusRequest.Month}/{bonusRequest.Year}.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
