using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Application.Holidays.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

using HR.Application.Common.Models;

namespace HR.Application.Holidays.Queries;

public record GetHolidaysQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<HolidayDto>>;

public class GetHolidaysQueryHandler : IRequestHandler<GetHolidaysQuery, PaginatedResult<HolidayDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHolidaysQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<HolidayDto>> Handle(GetHolidaysQuery request, CancellationToken cancellationToken)
    {
        var totalCount = await _context.Holidays.CountAsync(cancellationToken);

        var data = await _context.Holidays
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
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<HolidayDto>(data, totalCount, request.PageNumber, request.PageSize);
    }
}
