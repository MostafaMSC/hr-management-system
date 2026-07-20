using System;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Domain.Enums;
using HR.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Leaves.Commands;

// MANAGER COMMANDS
public record ApproveLeaveByManagerCommand(int LeaveId, int ManagerId, string? Comment) : IRequest<bool>;
public class ApproveLeaveByManagerCommandHandler : IRequestHandler<ApproveLeaveByManagerCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public ApproveLeaveByManagerCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(ApproveLeaveByManagerCommand request, CancellationToken cancellationToken)
    {
        var leave = await _context.LeaveRequests.FirstOrDefaultAsync(l => l.Id == request.LeaveId, cancellationToken);
        if (leave == null) throw new NotFoundException($"Leave request {request.LeaveId} not found.");

        if (leave.Status != LeaveStatus.AwaitingManagerApproval)
            throw new InvalidOperationException("Leave request is not awaiting manager approval.");

        leave.ApprovedByManagerId = request.ManagerId;
        leave.ManagerComment = request.Comment;
        leave.ApprovedByManagerAt = DateTime.UtcNow;

        // Next step is HR approval (unless it's a simple type that goes straight to approved, but generally we follow legacy logic)
        leave.Status = LeaveStatus.AwaitingHRApproval;
        leave.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record RejectLeaveByManagerCommand(int LeaveId, int ManagerId, string? Comment) : IRequest<bool>;
public class RejectLeaveByManagerCommandHandler : IRequestHandler<RejectLeaveByManagerCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public RejectLeaveByManagerCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(RejectLeaveByManagerCommand request, CancellationToken cancellationToken)
    {
        var leave = await _context.LeaveRequests.FirstOrDefaultAsync(l => l.Id == request.LeaveId, cancellationToken);
        if (leave == null) throw new NotFoundException($"Leave request {request.LeaveId} not found.");

        leave.RejectedById = request.ManagerId;
        leave.ManagerComment = request.Comment;
        leave.RejectedAt = DateTime.UtcNow;
        leave.Status = LeaveStatus.Rejected;
        leave.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// HR COMMANDS
public record ApproveLeaveByHRCommand(int LeaveId, int HrId, string? Comment) : IRequest<bool>;
public class ApproveLeaveByHRCommandHandler : IRequestHandler<ApproveLeaveByHRCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public ApproveLeaveByHRCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(ApproveLeaveByHRCommand request, CancellationToken cancellationToken)
    {
        var leave = await _context.LeaveRequests.FirstOrDefaultAsync(l => l.Id == request.LeaveId, cancellationToken);
        if (leave == null) throw new NotFoundException($"Leave request {request.LeaveId} not found.");

        if (leave.Status != LeaveStatus.AwaitingHRApproval)
            throw new InvalidOperationException("Leave request is not awaiting HR approval.");

        leave.ApprovedByHRId = request.HrId;
        leave.HrComment = request.Comment;
        leave.ApprovedByHRAt = DateTime.UtcNow;
        leave.Status = LeaveStatus.Approved;
        leave.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public record RejectLeaveByHRCommand(int LeaveId, int HrId, string? Comment) : IRequest<bool>;
public class RejectLeaveByHRCommandHandler : IRequestHandler<RejectLeaveByHRCommand, bool>
{
    private readonly IApplicationDbContext _context;
    public RejectLeaveByHRCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(RejectLeaveByHRCommand request, CancellationToken cancellationToken)
    {
        var leave = await _context.LeaveRequests.FirstOrDefaultAsync(l => l.Id == request.LeaveId, cancellationToken);
        if (leave == null) throw new NotFoundException($"Leave request {request.LeaveId} not found.");

        leave.RejectedById = request.HrId;
        leave.HrComment = request.Comment;
        leave.RejectedAt = DateTime.UtcNow;
        leave.Status = LeaveStatus.Rejected;
        leave.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
