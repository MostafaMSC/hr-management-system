using System;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Holidays.Commands;

public class DeleteHolidayCommand : IRequest<bool>
{
    public int Id { get; set; }
}

public class DeleteHolidayCommandHandler : IRequestHandler<DeleteHolidayCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IAttendanceEvaluationService _attendanceEvaluationService;

    public DeleteHolidayCommandHandler(
        IApplicationDbContext context,
        IAttendanceEvaluationService attendanceEvaluationService)
    {
        _context = context;
        _attendanceEvaluationService = attendanceEvaluationService;
    }

    public async Task<bool> Handle(DeleteHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = await _context.Holidays.FirstOrDefaultAsync(h => h.Id == request.Id, cancellationToken);

        if (holiday == null)
            throw new ArgumentException($"Holiday with ID {request.Id} not found.");

        _context.Holidays.Remove(holiday);
        await _context.SaveChangesAsync(cancellationToken);

        // Re-evaluate past holiday deletion
        if (holiday.Date.Date <= DateTime.Today)
        {
            await _attendanceEvaluationService.EvaluateAllUsersForDateAsync(holiday.Date.Date, cancellationToken);
        }

        return true;
    }
}
