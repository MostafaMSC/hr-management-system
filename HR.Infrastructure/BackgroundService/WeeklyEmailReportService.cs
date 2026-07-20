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
    /// Background service to send weekly attendance reports every Thursday
    /// </summary>
    public class WeeklyEmailReportService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WeeklyEmailReportService> _logger;

        public WeeklyEmailReportService(
            IServiceScopeFactory scopeFactory, 
            ILogger<WeeklyEmailReportService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("📧 WeeklyEmailReportService is starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    
                    // Check if today is Thursday (الخميس = 4)
                    if (now.DayOfWeek == DayOfWeek.Thursday)
                    {
                        // Check if it's 6:00 PM (18:00)
                        if (now.Hour == 18 && now.Minute < 5) // 5-minute window to execute
                        {
                            _logger.LogInformation("🕐 It's Thursday at 6:00 PM - Starting weekly report generation...");
                            await SendWeeklyReportsAsync(stoppingToken);
                            
                            // Wait 10 minutes to avoid re-execution in the same hour
                            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in WeeklyEmailReportService main loop");
                }

                // Check every 5 minutes
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("WeeklyEmailReportService is stopping...");
                    break;
                }
            }

            _logger.LogInformation("✅ WeeklyEmailReportService stopped.");
        }

        private async Task SendWeeklyReportsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                // Calculate week range: Saturday to Thursday
                var today = DateTime.Now.Date;
                
                // Find the last Saturday (start of the week)
                var daysToSubtract = ((int)today.DayOfWeek - (int)DayOfWeek.Saturday + 7) % 7;
                var weekStart = today.AddDays(-daysToSubtract);
                
                // End of week is today (Thursday)
                var weekEnd = today.AddDays(1); // Next day to include Thursday fully

                _logger.LogInformation("📅 Generating reports for week: {WeekStart:dd/MM/yyyy} to {WeekEnd:dd/MM/yyyy}", 
                    weekStart, weekEnd.AddDays(-1));

                // Get all users with email addresses
                var usersWithEmail = await context.UserInfos
                    .Where(u => !string.IsNullOrEmpty(u.Email) && u.AccountStatus == "Active")
                    .ToListAsync(cancellationToken);
                    
                _logger.LogInformation("👥 Found {Count} users with email addresses", usersWithEmail.Count);

                int successCount = 0;
                int failureCount = 0;

                foreach (var user in usersWithEmail)
                {
                    try
                    {
                        // Skip users without BiometricId
                        if (string.IsNullOrEmpty(user.BiometricId))
                        {
                            _logger.LogWarning("⚠️ User {Username} has no BiometricId, skipping...", user.Username);
                            continue;
                        }

                        // Get weekly logs for this user
                        var weeklyLogs = await context.AttendanceLogs
                            .Where(a => a.UserInfoId == user.Id && a.Time >= weekStart && a.Time < weekEnd)
                            .ToListAsync(cancellationToken);

                        // Skip if no logs found
                        if (!weeklyLogs.Any())
                        {
                            _logger.LogInformation("ℹ️ No attendance logs found for user {Username} this week, skipping email", user.Username);
                            continue;
                        }

                        // Send the weekly report email
                        await emailService.SendWeeklyAttendanceReportAsync(
                            user.Email!,
                            user.Username,
                            weeklyLogs,
                            weekStart,
                            weekEnd.AddDays(-1), // Show Thursday as end date, not Friday
                            cancellationToken
                        );

                        _logger.LogInformation("✅ Weekly report sent successfully to {Username} ({Email})", 
                            user.Username, user.Email);

                        // Create notification for the user
                        await notificationService.NotifyWeeklyReportSentAsync(
                            user.Id,
                            weekStart,
                            weekEnd.AddDays(-1));

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Failed to send weekly report to user {Username} ({Email})", 
                            user.Username, user.Email);
                        failureCount++;
                    }

                    // Small delay between emails to avoid overwhelming SMTP server
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                }

                _logger.LogInformation(
                    "📊 Weekly report batch completed: {SuccessCount} successful, {FailureCount} failed",
                    successCount, failureCount
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Critical error in SendWeeklyReportsAsync");
            }
        }
    }
}
