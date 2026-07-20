using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Application.Holidays.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Holidays.Queries;

public record GetHolidayByIdQuery(int Id) : IRequest<HolidayDto?>;

public class GetHolidayByIdQueryHandler : IRequestHandler<GetHolidayByIdQuery, HolidayDto?>
{
    private readonly IApplicationDbContext _context;

    public GetHolidayByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HolidayDto?> Handle(GetHolidayByIdQuery request, CancellationToken cancellationToken)
    {
        var holiday = await _context.Holidays
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == request.Id, cancellationToken);

        if (holiday == null) return null;

        return new HolidayDto
        {
            Id = holiday.Id,
            Name = holiday.Name,
            Date = holiday.Date,
            CreatedAt = holiday.CreatedAt,
            UpdatedAt = holiday.UpdatedAt
        };
    }
}
