using HR.Application.Attendance.Shifts.DTOs;
using HR.Application.Common.Interfaces;
using HR.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Attendance.Shifts.Queries;

public record GetShiftByIdQuery(int Id) : IRequest<AttendanceShiftDto>;

public class GetShiftByIdQueryHandler : IRequestHandler<GetShiftByIdQuery, AttendanceShiftDto>
{
    private readonly IApplicationDbContext _context;

    public GetShiftByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceShiftDto> Handle(GetShiftByIdQuery request, CancellationToken cancellationToken)
    {
        var shift = await _context.AttendanceShifts
            .Where(s => s.Id == request.Id && !s.IsDeleted)
            .Select(s => new AttendanceShiftDto
            {
                Id = s.Id,
                Name = s.Name,
                StartTime = s.StartTime.ToString(@"hh\:mm"),
                EndTime = s.EndTime.ToString(@"hh\:mm"),
                UserCount = s.Users.Count(u => !u.IsDeleted),
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (shift == null)
            throw new NotFoundException($"Shift with ID {request.Id} not found.");

        return shift;
    }
}
