using HR.Domain.Enums;

namespace HR.Application.Notifications.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public NotificationType Type { get; set; }
    public string? Link { get; set; }
    public string? Data { get; set; }
    public int? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
