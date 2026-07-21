using HR.Application.Attendance.Shifts.DTOs;
using HR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

using HR.Application.Common.Models;

namespace HR.Application.Attendance.Shifts.Queries;

public record GetShiftsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<AttendanceShiftDto>>;

public class GetShiftsQueryHandler : IRequestHandler<GetShiftsQuery, PaginatedResult<AttendanceShiftDto>>
{
    private readonly IApplicationDbContext _context;

    public GetShiftsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<AttendanceShiftDto>> Handle(GetShiftsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AttendanceShifts.Where(s => !s.IsDeleted);
        var totalCount = await query.CountAsync(cancellationToken);

        var data = await query
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
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AttendanceShiftDto>(data, totalCount, request.PageNumber, request.PageSize);
    }
}
