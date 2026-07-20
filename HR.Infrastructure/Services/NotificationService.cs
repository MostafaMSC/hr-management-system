using System;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Domain.Enums;
namespace HR.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        public Task SendNotificationAsync(int userId, string message) => Task.CompletedTask;
        public Task CreateNotificationAsync(int userId, string title, string message, NotificationType type, string? referenceId = null, string? referenceType = null) => Task.CompletedTask;
        public Task NotifyLeaveStatusChangedAsync(int userId, int leaveRequestId, LeaveStatus status, string? adminComments = null) => Task.CompletedTask;
        public Task NotifyWeeklyReportSentAsync(int userId, DateTime startDate, DateTime endDate) => Task.CompletedTask;
        public Task NotifyMissingHRAsync(int userId) => Task.CompletedTask;
    }
}
