using System;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace HR.Infrastructure.Services;

public class AttendanceEvaluationService : IAttendanceEvaluationService
{
    private readonly ILogger<AttendanceEvaluationService> _logger;

    public AttendanceEvaluationService(ILogger<AttendanceEvaluationService> logger)
    {
        _logger = logger;
    }

    public Task EvaluateAllUsersForDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        // TODO: Full implementation for attendance evaluation.
        // For now, this is a stub so that Holidays can be created and the system compiles.
        _logger.LogInformation("Stub: Evaluating attendance for all users on date {Date}", date.ToShortDateString());
        return Task.CompletedTask;
    }
}
