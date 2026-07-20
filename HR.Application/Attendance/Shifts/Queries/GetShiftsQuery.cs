using HR.Application.Attendance.Shifts.DTOs;
using HR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Attendance.Shifts.Queries;

public record GetShiftsQuery : IRequest<List<AttendanceShiftDto>>;

public class GetShiftsQueryHandler : IRequestHandler<GetShiftsQuery, List<AttendanceShiftDto>>
{
    private readonly IApplicationDbContext _context;

    public GetShiftsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AttendanceShiftDto>> Handle(GetShiftsQuery request, CancellationToken cancellationToken)
    {
        return await _context.AttendanceShifts
            .Where(s => !s.IsDeleted)
            .Select(s => new AttendanceShiftDto
            {
                Id = s.Id,
                Name = s.Name,
                StartTime = s.StartTime.ToString(@"hh\:mm"),
                EndTime = s.EndTime.ToString(@"hh\:mm"),
                LateThreshold = s.LateThreshold.HasValue ? s.LateThreshold.Value.ToString(@"hh\:mm") : null,
                UserCount = s.Users.Count(u => !u.IsDeleted),
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
