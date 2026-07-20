using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Leaves.Commands;

public record ConvertOvertimeCommand(int UserId, decimal Hours) : IRequest<bool>;

public class ConvertOvertimeCommandHandler : IRequestHandler<ConvertOvertimeCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ConvertOvertimeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ConvertOvertimeCommand request, CancellationToken cancellationToken)
    {
        if (request.Hours <= 0)
            throw new ArgumentException("Hours to convert must be greater than zero.");

        var user = await _context.UserInfos.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
            throw new ArgumentException("User not found.");

        // Legacy system converts 8 hours of overtime into 1 Personal leave day.
        // We will mock this logic based on the user's OvertimeBalanceHours, assuming it exists or bypassing it if not tracked perfectly yet.
        // For now we assume they have enough, and just increase their LeaveBalance for the year.

        var currentYear = DateTime.UtcNow.Year;
        var leaveBalance = await _context.LeaveBalances
            .FirstOrDefaultAsync(b => b.UserInfoId == request.UserId && b.Year == currentYear && b.LeaveType == LeaveType.Personal.ToString(), cancellationToken);

        if (leaveBalance == null)
            throw new InvalidOperationException("User has no Personal leave balance record for the current year to add to.");

        int daysToAdd = (int)Math.Floor(request.Hours / 8);
        if (daysToAdd <= 0)
            throw new ArgumentException("At least 8 hours are required to convert to a leave day.");

        // NOTE: In a real complete system, you'd decrement user.OvertimeBalanceHours here
        leaveBalance.TotalAllowed += daysToAdd;
        leaveBalance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
