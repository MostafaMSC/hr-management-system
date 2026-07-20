using HR.Application.Attendance.Commands;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HR.Infrastructure.Services;

public class LegacyDevicePollingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LegacyDevicePollingService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromMinutes(5);

    public LegacyDevicePollingService(IServiceProvider serviceProvider, ILogger<LegacyDevicePollingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Legacy Device Polling Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollDevicesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while polling legacy devices.");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Legacy Device Polling Service is stopping.");
    }

    private async Task PollDevicesAsync(CancellationToken cancellationToken)
    {
        // Create a new scope to resolve scoped services (like DbContext and Mediator) inside a singleton BackgroundService
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var attendanceProvider = scope.ServiceProvider.GetRequiredService<IAttendanceProvider>();

        // 1. Fetch all active legacy devices (e.g., ZKTeco machines requiring polling)
        var devicesToPoll = await context.Devices
            .Where(d => d.IsActive) // In a real app, you might have a flag like d.RequiresPolling
            .ToListAsync(cancellationToken);

        foreach (var device in devicesToPoll)
        {
            try
            {
                _logger.LogInformation("Polling device {IpAddress}...", device.IpAddress);

                // 2. Fetch logs from the Python Microservice/Edge Agent using the abstract provider
                var logs = await attendanceProvider.FetchLogsAsync(device.IpAddress, cancellationToken);

                // 3. Process each log through the unified pipeline
                int processedCount = 0;
                foreach (var log in logs)
                {
                    // Ensure the IP matches the device we just polled
                    log.DeviceIP = device.IpAddress;

                    var command = new ProcessAttendanceLogCommand { Log = log };
                    var success = await mediator.Send(command, cancellationToken);
                    if (success) processedCount++;
                }

                _logger.LogInformation("Successfully polled and processed {Count} logs from device {IpAddress}.", processedCount, device.IpAddress);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to poll device {IpAddress}.", device.IpAddress);
            }
        }
    }
}
