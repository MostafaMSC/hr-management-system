using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HR.Application.Common.Interfaces;
using HR.Domain.Enums;
using HR.Application.Attendance.DTOs;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HR.WebApi.Services
{
    public class DeviceSyncBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DeviceSyncBackgroundService> _logger;

        public DeviceSyncBackgroundService(IServiceProvider serviceProvider, ILogger<DeviceSyncBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Device Sync Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingTasksAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing Device Sync Background Service.");
                }

                // Wait 15 minutes before running again
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }

        private async Task ProcessPendingTasksAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var deviceProviderFactory = scope.ServiceProvider.GetRequiredService<IDeviceProviderFactory>();
            var fingerprintRepository = scope.ServiceProvider.GetRequiredService<IFingerprintRepository>();

            var pendingTasks = await dbContext.DeviceSyncTasks
                .Include(t => t.Device)
                .Include(t => t.UserInfo)
                .Where(t => t.Status == SyncTaskStatus.Pending)
                .OrderBy(t => t.CreatedAt)
                .Take(50) // Process in batches
                .ToListAsync(cancellationToken);

            if (!pendingTasks.Any()) return;

            _logger.LogInformation("Found {Count} pending device sync tasks.", pendingTasks.Count);

            foreach (var task in pendingTasks)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    var provider = deviceProviderFactory.GetProvider(task.Device.Protocol);
                    bool success = false;
                    
                    if (string.IsNullOrEmpty(task.UserInfo.BiometricId))
                    {
                        task.ErrorMessage = "BiometricId is empty.";
                        task.Status = SyncTaskStatus.Failed;
                        continue;
                    }

                    if (task.Action == SyncAction.Add)
                    {
                        var userFingerprints = await fingerprintRepository.GetByUserIdAsync(task.UserId, cancellationToken);
                        
                        var syncUserDto = new SyncUserRequestDto
                        {
                            UserId = task.UserInfo.BiometricId,
                            Name = task.UserInfo.Username ?? $"{task.UserInfo.FirstName} {task.UserInfo.LastName}",
                            Password = task.UserInfo.Password,
                            Card = task.UserInfo.Card ?? "0",
                            Privilege = task.UserInfo.Role == UserType.Administrator ? 14 : 0,
                            Fingerprints = userFingerprints.Select(f => new DeviceFingerprintDto
                            {
                                FingerIndex = f.FingerIndex,
                                Template = Convert.ToBase64String(f.Template)
                            }).ToList()
                        };

                        success = await provider.SyncFullUserAsync(task.Device.IpAddress, syncUserDto, cancellationToken);
                    }
                    else if (task.Action == SyncAction.Delete)
                    {
                        success = await provider.DeleteUserAsync(task.Device.IpAddress, task.UserInfo.BiometricId, cancellationToken);
                    }

                    if (success)
                    {
                        task.Status = SyncTaskStatus.Completed;
                        task.ErrorMessage = null;
                        _logger.LogInformation("Successfully synced task {TaskId} for user {UserId} on device {DeviceId}.", task.Id, task.UserId, task.DeviceId);
                    }
                    else
                    {
                        task.RetryCount++;
                        task.ErrorMessage = "Device provider returned false.";
                        _logger.LogWarning("Failed to sync task {TaskId}. Retry count: {RetryCount}", task.Id, task.RetryCount);
                    }
                }
                catch (Exception ex)
                {
                    task.RetryCount++;
                    task.ErrorMessage = ex.Message;
                    _logger.LogError(ex, "Exception while processing task {TaskId}", task.Id);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
