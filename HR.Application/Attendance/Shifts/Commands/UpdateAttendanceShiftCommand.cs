using HR.Application.Attendance.Shifts.DTOs;
using HR.Application.Common.Interfaces;
using HR.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Attendance.Shifts.Commands;

public record UpdateAttendanceShiftCommand(int Id, string? Name, string? StartTime, string? EndTime, string? LateThreshold = null) : IRequest<AttendanceShiftDto>;

public class UpdateAttendanceShiftCommandHandler : IRequestHandler<UpdateAttendanceShiftCommand, AttendanceShiftDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateAttendanceShiftCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceShiftDto> Handle(UpdateAttendanceShiftCommand request, CancellationToken cancellationToken)
    {
        var shift = await _context.AttendanceShifts.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (shift == null)
            throw new NotFoundException($"Shift with ID {request.Id} not found.");

        if (request.Name != null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Shift name cannot be empty.");
            shift.Name = request.Name;
        }

        if (request.StartTime != null)
        {
            if (!TimeSpan.TryParse(request.StartTime, out TimeSpan startTime))
                throw new ArgumentException("Invalid StartTime format. Use HH:mm.");
            shift.StartTime = startTime;
        }

        if (request.EndTime != null)
        {
            if (!TimeSpan.TryParse(request.EndTime, out TimeSpan endTime))
                throw new ArgumentException("Invalid EndTime format. Use HH:mm.");
            shift.EndTime = endTime;
        }

        if (request.LateThreshold != null)
        {
            if (string.IsNullOrWhiteSpace(request.LateThreshold))
            {
                shift.LateThreshold = null;
            }
            else if (!TimeSpan.TryParse(request.LateThreshold, out TimeSpan lateThreshold))
            {
                throw new ArgumentException("Invalid LateThreshold format. Use HH:mm.");
            }
            else
            {
                shift.LateThreshold = lateThreshold;
            }
        }

        shift.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new AttendanceShiftDto
        {
            Id = shift.Id,
            Name = shift.Name,
            StartTime = shift.StartTime.ToString(@"hh\:mm"),
            EndTime = shift.EndTime.ToString(@"hh\:mm"),
            LateThreshold = shift.LateThreshold?.ToString(@"hh\:mm"),
            UserCount = await _context.UserInfos.CountAsync(u => u.AttendanceShiftId == shift.Id && !u.IsDeleted, cancellationToken),
            CreatedAt = shift.CreatedAt,
            UpdatedAt = shift.UpdatedAt
        };
    }
}
