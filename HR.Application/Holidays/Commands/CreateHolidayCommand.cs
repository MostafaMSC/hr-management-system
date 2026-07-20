using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Application.Holidays.Commands;

public class CreateHolidayCommand : IRequest<int>
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CreateHolidayCommandHandler : IRequestHandler<CreateHolidayCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IAttendanceEvaluationService _attendanceEvaluationService;
    private readonly INotificationService _notificationService;

    public CreateHolidayCommandHandler(
        IApplicationDbContext context,
        IAttendanceEvaluationService attendanceEvaluationService,
        INotificationService notificationService)
    {
        _context = context;
        _attendanceEvaluationService = attendanceEvaluationService;
        _notificationService = notificationService;
    }

    public async Task<int> Handle(CreateHolidayCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Holiday name is required.");

        var startDate = request.Date.Date;
        var endDate = request.EndDate.HasValue ? request.EndDate.Value.Date : startDate;

        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date.");

        var holidaysToCreate = new List<Holiday>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            holidaysToCreate.Add(new Holiday
            {
                Name = request.Name,
                Date = DateTime.SpecifyKind(date, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _context.Holidays.AddRange(holidaysToCreate);
        await _context.SaveChangesAsync(cancellationToken);

        // Re-evaluate past holidays
        foreach (var h in holidaysToCreate)
        {
            if (h.Date.Date <= DateTime.Today)
            {
                await _attendanceEvaluationService.EvaluateAllUsersForDateAsync(h.Date.Date, cancellationToken);
            }
        }

        // Create notification message text
        string messageText = startDate == endDate
            ? $"Holiday on {startDate:dd/MM/yyyy}."
            : $"Holiday from {startDate:dd/MM/yyyy} to {endDate:dd/MM/yyyy}.";

        // Insert in notifications table (one record per active user)
        var activeUsers = await _context.UserInfos
            .Where(u => u.AccountStatus == "Active")
            .ToListAsync(cancellationToken);

        foreach (var user in activeUsers)
        {
            await _notificationService.CreateNotificationAsync(
                userId: user.Id,
                title: $"Holiday: {request.Name}",
                message: messageText,
                type: NotificationType.Holiday
            );
        }

        return holidaysToCreate.First().Id;
    }
}
