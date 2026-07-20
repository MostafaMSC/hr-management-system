using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Application.Holidays.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Holidays.Queries;

public record GetHolidaysQuery : IRequest<List<HolidayDto>>;

public class GetHolidaysQueryHandler : IRequestHandler<GetHolidaysQuery, List<HolidayDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHolidaysQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HolidayDto>> Handle(GetHolidaysQuery request, CancellationToken cancellationToken)
    {
        return await _context.Holidays
            .AsNoTracking()
            .OrderBy(h => h.Date)
            .Select(h => new HolidayDto
            {
                Id = h.Id,
                Name = h.Name,
                Date = h.Date,
                CreatedAt = h.CreatedAt,
                UpdatedAt = h.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
