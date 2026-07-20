using System;
using System.Text.Json;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace HR.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IApplicationDbContext _context;
        private readonly IHubContext<RealTimeHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IApplicationDbContext context,
            IHubContext<RealTimeHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task CreateNotificationAsync(
            int userId,
            string title,
            string message,
            NotificationType type,
            string? link = null,
            string? data = null)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    Link = link,
                    Data = data,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync(default);

                // Send real-time notification via SignalR
                await _hubContext.Clients.User(userId.ToString())
                    .SendAsync("ReceiveNotification", new
                    {
                        id = notification.Id,
                        title = notification.Title,
                        message = notification.Message,
                        type = notification.Type.ToString(),
                        link = notification.Link,
                        isRead = notification.IsRead,
                        createdAt = notification.CreatedAt
                    });

                _logger.LogInformation(
                    "✅ Notification created and sent to user {UserId}: {Title}",
                    userId, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "❌ Error creating notification for user {UserId}: {Title}", 
                    userId, title);
            }
        }

        public async Task NotifyLeaveStatusChangedAsync(
            int employeeId,
            int leaveRequestId,
            LeaveStatus newStatus,
            string? comment = null)
        {
            string title;
            string message;
            string link = $"/dashboard?tab=myLeaveRequests&leaveId={leaveRequestId}";

            switch (newStatus)
            {
                case LeaveStatus.AwaitingManagerApproval:
                    title = "Leave Request Pending";
                    message = string.IsNullOrWhiteSpace(comment)
                        ? "Your leave request is pending approval."
                        : comment;
                    break;

                case LeaveStatus.AwaitingHRApproval:
                case LeaveStatus.AwaitingSecondLineApproval:
                case LeaveStatus.Approved:
                    title = "Leave Request Approved";
                    message = "Your leave request has been fully approved. ✅";
                    if (!string.IsNullOrEmpty(comment))
                        message += $"\n\nComment: {comment}";
                    break;

                case LeaveStatus.Rejected:
                    title = "Leave Request Rejected";
                    message = "Your leave request has been rejected. ❌";
                    if (!string.IsNullOrEmpty(comment))
                        message += $"\n\nReason: {comment}";
                    break;

                default:
                    title = "Leave Request Status Updated";
                    message = $"Your leave request status has been updated to: {newStatus}";
                    break;
            }

            var data = JsonSerializer.Serialize(new
            {
                leaveRequestId,
                newStatus = newStatus.ToString(),
                comment
            });

            await CreateNotificationAsync(
                employeeId,
                title,
                message,
                NotificationType.LeaveStatusChanged,
                link,
                data);
        }

        public async Task NotifyWeeklyReportSentAsync(
            int userId,
            DateTime weekStart,
            DateTime weekEnd)
        {
            var title = "📊 Weekly Attendance Report Sent";
            var message = $"Your weekly attendance report for {weekStart:dd/MM/yyyy} - {weekEnd:dd/MM/yyyy} has been sent to your email.";

            var data = JsonSerializer.Serialize(new
            {
                weekStart,
                weekEnd,
                reportType = "weekly"
            });

            await CreateNotificationAsync(
                userId,
                title,
                message,
                NotificationType.WeeklyReportSent,
                link: null,
                data);
        }

        public async Task NotifyMissingFingerprintAsync(int userId)
        {
            var title = "⚠️ Missing Fingerprint Check-In";
            var message = "You have not recorded your fingerprint today. Please check in as soon as possible.";

            var data = JsonSerializer.Serialize(new
            {
                date = DateTime.UtcNow.Date,
                reminderType = "missing_checkin"
            });

            await CreateNotificationAsync(
                userId,
                title,
                message,
                NotificationType.MissingFingerprint,
                link: "/attendance",
                data);
        }
    }
}
