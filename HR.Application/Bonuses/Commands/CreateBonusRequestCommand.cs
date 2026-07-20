using System;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Domain.Common;
using HR.Domain.Entities;
using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Bonuses.Commands;

public record CreateBonusRequestCommand(
    int RequestingManagerId,
    int TargetUserId,
    BonusType Type,
    decimal Value,
    string Reason,
    int Year,
    int Month) : IRequest<Result<int>>;

public class CreateBonusRequestCommandHandler : IRequestHandler<CreateBonusRequestCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;

    public CreateBonusRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(CreateBonusRequestCommand request, CancellationToken cancellationToken)
    {
        var manager = await _context.UserInfos.FindAsync(new object[] { request.RequestingManagerId }, cancellationToken);
        var targetUser = await _context.UserInfos.FindAsync(new object[] { request.TargetUserId }, cancellationToken);

        if (manager == null) return Result<int>.Failure("Manager not found.");
        if (targetUser == null) return Result<int>.Failure("Target user not found.");

        var bonusRequest = new BonusRequest
        {
            RequestingManagerId = request.RequestingManagerId,
            TargetUserId = request.TargetUserId,
            Type = request.Type,
            Value = request.Value,
            Reason = request.Reason,
            Year = request.Year,
            Month = request.Month,
            Status = BonusStatus.Pending
        };

        _context.BonusRequests.Add(bonusRequest);

        // Add Notification for HR
        var hrUsers = await _context.UserInfos
            .Where(u => u.Role == UserType.HR || u.Role == UserType.Administrator)
            .ToListAsync(cancellationToken);

        foreach (var hr in hrUsers)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = hr.Id,
                Title = "New Bonus Request",
                Message = $"Manager {manager.Username} requested a bonus for {targetUser.Username}.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(bonusRequest.Id);
    }
}
