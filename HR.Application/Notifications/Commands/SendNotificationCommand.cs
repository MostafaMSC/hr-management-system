using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
using MediatR;

namespace HR.Application.Notifications.Commands;

public record SendNotificationCommand : IRequest<bool>
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string? Link { get; set; }
    public string? Data { get; set; }
    public int? UserId { get; set; } // Null for global broadcast
}

public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, bool>
{
    private readonly INotificationRepository _notificationRepository;

    public SendNotificationCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<bool> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Title = request.Title,
            Message = request.Message,
            Type = request.Type,
            Link = request.Link,
            Data = request.Data,
            IsRead = false,
            UserId = request.UserId
        };

        await _notificationRepository.CreateAsync(notification);

        // Push notification via SignalR/Firebase could happen here
        // _pushService.SendToUser(request.UserId, ...);

        return true;
    }
}
