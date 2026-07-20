using System;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Holidays.Commands;

public class UpdateHolidayCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

public class UpdateHolidayCommandHandler : IRequestHandler<UpdateHolidayCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IAttendanceEvaluationService _attendanceEvaluationService;

    public UpdateHolidayCommandHandler(
        IApplicationDbContext context,
        IAttendanceEvaluationService attendanceEvaluationService)
    {
        _context = context;
        _attendanceEvaluationService = attendanceEvaluationService;
    }

    public async Task<bool> Handle(UpdateHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = await _context.Holidays.FirstOrDefaultAsync(h => h.Id == request.Id, cancellationToken);

        if (holiday == null)
            throw new ArgumentException($"Holiday with ID {request.Id} not found.");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Holiday name is required.");

        holiday.Name = request.Name;
        holiday.Date = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Utc);
        holiday.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Re-evaluate past holiday
        if (holiday.Date.Date <= DateTime.Today)
        {
            await _attendanceEvaluationService.EvaluateAllUsersForDateAsync(holiday.Date.Date, cancellationToken);
        }

        return true;
    }
}
