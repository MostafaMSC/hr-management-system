namespace HR.Application.Common.Interfaces;

public interface IZKTecoService
{
    Task SyncAttendanceLogsAsync(CancellationToken cancellationToken);
}
