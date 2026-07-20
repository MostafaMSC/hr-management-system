using System;
using System.Threading;
using System.Threading.Tasks;

namespace HR.Application.Common.Interfaces;

public interface IAttendanceEvaluationService
{
    Task EvaluateAllUsersForDateAsync(DateTime date, CancellationToken cancellationToken = default);
}
