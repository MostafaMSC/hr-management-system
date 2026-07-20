using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HR.Infrastructure.BackgroundService
{
    /// <summary>
    /// Background service to check daily for missing fingerprints after 9 AM
    /// </summary>
    public class DailyFingerprintCheckService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DailyFingerprintCheckService> _logger;

        public DailyFingerprintCheckService(
            IServiceScopeFactory scopeFactory, 
            ILogger<DailyFingerprintCheckService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("📋 DailyFingerprintCheckService is starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    
                    // Check if it's after 9:00 AM (with 5-minute window for execution)
                    if (now.Hour == 9 && now.Minute < 5)
                    {
                        _logger.LogInformation("🕐 It's 9:00 AM - Starting missing fingerprint check...");
                        await CheckMissingFingerprintsAsync(stoppingToken);
                        
                        // Wait 10 minutes to avoid re-execution in the same hour
                        await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in DailyFingerprintCheckService main loop");
                }

                // Check every 5 minutes
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("DailyFingerprintCheckService is stopping...");
                    break;
                }
            }

            _logger.LogInformation("✅ DailyFingerprintCheckService stopped.");
        }

        private async Task CheckMissingFingerprintsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                var today = DateTime.Now.Date;
                var todayStart = today;
                var todayEnd = today.AddDays(1);

                _logger.LogInformation("📅 Checking for missing fingerprints on: {Date:dd/MM/yyyy}", today);

                // Get all active users
                var allUsers = await context.UserInfos
                    .Where(u => u.AccountStatus == "Active")
                    .ToListAsync(cancellationToken);
                    
                _logger.LogInformation("👥 Found {Count} total active users", allUsers.Count);

                int notifiedCount = 0;
                int skippedCount = 0;

                foreach (var user in allUsers)
                {
                    try
                    {
                        // Skip users without BiometricId
                        if (string.IsNullOrEmpty(user.BiometricId))
                        {
                            skippedCount++;
                            continue;
                        }

                        // Check if user has any attendance log today
                        var hasLogToday = await context.AttendanceLogs
                            .AnyAsync(a => a.UserInfoId == user.Id && a.Time >= todayStart && a.Time < todayEnd, cancellationToken);

                        // If no logs found, send notification
                        if (!hasLogToday)
                        {
                            await notificationService.NotifyMissingFingerprintAsync(user.Id);
                            
                            _logger.LogInformation(
                                "⚠️ Notification sent to {Username} (ID: {UserId}) - No fingerprint today",
                                user.Username, user.Id);
                            notifiedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, 
                            "❌ Error checking fingerprint for user {Username} (ID: {UserId})", 
                            user.Username, user.Id);
                    }

                    // Small delay between users to avoid overwhelming the system
                    await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                }

                _logger.LogInformation(
                    "📊 Missing fingerprint check completed: {NotifiedCount} notified, {SkippedCount} skipped (no BiometricId)",
                    notifiedCount, skippedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Critical error in CheckMissingFingerprintsAsync");
            }
        }
    }
}
