using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Application.Leaves.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Leaves.Queries;

public record GetLeaveRequestByIdQuery(int Id) : IRequest<LeaveRequestDto?>;

public class GetLeaveRequestByIdQueryHandler : IRequestHandler<GetLeaveRequestByIdQuery, LeaveRequestDto?>
{
    private readonly IApplicationDbContext _context;

    public GetLeaveRequestByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveRequestDto?> Handle(GetLeaveRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var l = await _context.LeaveRequests
            .Include(x => x.UserInfo)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (l == null) return null;

        return new LeaveRequestDto
        {
            Id = l.Id,
            EmployeeId = l.UserInfoId,
            EmployeeName = $"{l.UserInfo.FirstName} {l.UserInfo.LastName}",
            LeaveType = l.LeaveType.ToString(),
            RequestedShiftId = l.RequestedShiftId,
            LeaveReason = l.LeaveReason,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            LeaveDate = l.LeaveDate,
            StartTime = l.StartTime.HasValue ? l.StartTime.Value.ToString(@"hh\:mm") : null,
            EndTime = l.EndTime.HasValue ? l.EndTime.Value.ToString(@"hh\:mm") : null,
            Reason = l.Reason,
            Status = l.Status.ToString(),
            ManagerComment = l.ManagerComment,
            HrComment = l.HrComment,
            AttachmentUrl = l.AttachmentUrl,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt
        };
    }
}
