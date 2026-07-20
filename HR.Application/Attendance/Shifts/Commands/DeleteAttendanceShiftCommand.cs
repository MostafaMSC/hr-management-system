using HR.Application.Common.Interfaces;
using HR.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Attendance.Shifts.Commands;

public record DeleteAttendanceShiftCommand(int Id) : IRequest<Unit>;

public class DeleteAttendanceShiftCommandHandler : IRequestHandler<DeleteAttendanceShiftCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteAttendanceShiftCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteAttendanceShiftCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == 1)
            throw new ArgumentException("The default Normal Shift cannot be deleted.");

        var shift = await _context.AttendanceShifts.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (shift == null)
            throw new NotFoundException($"Shift with ID {request.Id} not found.");

        // Soft delete shift
        shift.IsDeleted = true;
        shift.DeletedAt = DateTime.UtcNow;
        shift.UpdatedAt = DateTime.UtcNow;

        // Reassign users of this shift to default Normal Shift (ID = 1)
        var users = await _context.UserInfos.Where(u => u.AttendanceShiftId == request.Id).ToListAsync(cancellationToken);
        foreach (var user in users)
        {
            user.AttendanceShiftId = 1;
        }

        await _context.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
