using HR.Domain.Entities;

namespace HR.Application.Common.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> CreateAsync(Notification notification);
        Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);
        Task<int> GetUnreadCountAsync(int userId);
        Task<Notification?> GetByIdAsync(int id);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteAsync(int notificationId);
    }
}
