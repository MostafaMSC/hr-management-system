using HR.Domain.Entities;
using HR.Domain.Enums;

namespace HR.Application.Common.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(
            int userId,
            string title,
            string message,
            NotificationType type,
            string? link = null,
            string? data = null);

        Task NotifyLeaveStatusChangedAsync(
            int employeeId,
            int leaveRequestId,
            LeaveStatus newStatus,
            string? comment = null);

        Task NotifyWeeklyReportSentAsync(
            int userId,
            DateTime weekStart,
            DateTime weekEnd);

        Task NotifyMissingHRAsync(int userId);
    }
}
