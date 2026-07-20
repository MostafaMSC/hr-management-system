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

public class CreateLeaveRequestCommand : IRequest<int>
{
    public int UserId { get; set; }
    public LeaveType Type { get; set; }
    public string? LeaveReason { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? LeaveDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsManagerRole { get; set; }
    public int? RequestedShiftId { get; set; }

    public CreateLeaveRequestCommand(
        int userId,
        LeaveType type,
        string? leaveReason,
        DateTime? startDate,
        DateTime? endDate,
        DateTime? leaveDate,
        TimeSpan? startTime,
        TimeSpan? endTime,
        string reason,
        bool isManagerRole,
        int? requestedShiftId)
    {
        UserId = userId;
        Type = type;
        LeaveReason = leaveReason;
        StartDate = startDate;
        EndDate = endDate;
        LeaveDate = leaveDate;
        StartTime = startTime;
        EndTime = endTime;
        Reason = reason ?? string.Empty;
        IsManagerRole = isManagerRole;
        RequestedShiftId = requestedShiftId;
    }
}

public class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public CreateLeaveRequestCommandHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<int> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = new LeaveRequest
        {
            UserInfoId = request.UserId,
            LeaveType = request.Type,
            LeaveReason = request.LeaveReason,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            LeaveDate = request.LeaveDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Reason = request.Reason,
            RequestedShiftId = request.RequestedShiftId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // If employee is a manager, they can bypass manager approval and go straight to HR/SecondLine
        leaveRequest.Status = request.IsManagerRole ? LeaveStatus.AwaitingHRApproval : LeaveStatus.AwaitingManagerApproval;

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync(cancellationToken);

        // Notify managers or HR
        if (request.IsManagerRole)
        {
            var hrUsers = await _context.UserInfos
                .Where(u => !u.IsDeleted && u.Role == UserType.HR)
                .ToListAsync(cancellationToken);

            foreach (var hr in hrUsers)
            {
                await _notificationService.CreateNotificationAsync(
                    hr.Id,
                    "New Leave Request",
                    $"A new leave request awaits HR approval (ID: {leaveRequest.Id}).",
                    NotificationType.LeaveStatusChanged);
            }
        }
        else
        {
            var user = await _context.UserInfos
                .Include(u => u.DirectManager)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user?.DirectManagerId != null)
            {
                await _notificationService.CreateNotificationAsync(
                    user.DirectManagerId.Value,
                    "New Leave Request",
                    $"Employee {user.FirstName} {user.LastName} has requested leave (ID: {leaveRequest.Id}).",
                    NotificationType.LeaveStatusChanged);
            }
        }

        return leaveRequest.Id;
    }
}
