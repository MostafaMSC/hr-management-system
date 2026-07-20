using HR.Application.Attendance.Shifts.DTOs;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using MediatR;

namespace HR.Application.Attendance.Shifts.Commands;

public record CreateAttendanceShiftCommand(string Name, string StartTime, string EndTime, string? LateThreshold = null) : IRequest<AttendanceShiftDto>;

public class CreateAttendanceShiftCommandHandler : IRequestHandler<CreateAttendanceShiftCommand, AttendanceShiftDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAttendanceShiftCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceShiftDto> Handle(CreateAttendanceShiftCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Shift name is required.");

        if (!TimeSpan.TryParse(request.StartTime, out TimeSpan startTime))
            throw new ArgumentException("Invalid StartTime format. Use HH:mm.");

        if (!TimeSpan.TryParse(request.EndTime, out TimeSpan endTime))
            throw new ArgumentException("Invalid EndTime format. Use HH:mm.");

        TimeSpan? lateThreshold = null;
        if (!string.IsNullOrWhiteSpace(request.LateThreshold))
        {
            if (!TimeSpan.TryParse(request.LateThreshold, out TimeSpan lt))
                throw new ArgumentException("Invalid LateThreshold format. Use HH:mm.");
            lateThreshold = lt;
        }

        var shift = new AttendanceShift
        {
            Name = request.Name,
            StartTime = startTime,
            EndTime = endTime,
            LateThreshold = lateThreshold,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AttendanceShifts.Add(shift);
        await _context.SaveChangesAsync(cancellationToken);

        return new AttendanceShiftDto
        {
            Id = shift.Id,
            Name = shift.Name,
            StartTime = shift.StartTime.ToString(@"hh\:mm"),
            EndTime = shift.EndTime.ToString(@"hh\:mm"),
            LateThreshold = shift.LateThreshold?.ToString(@"hh\:mm"),
            UserCount = 0,
            CreatedAt = shift.CreatedAt,
            UpdatedAt = shift.UpdatedAt
        };
    }
}
