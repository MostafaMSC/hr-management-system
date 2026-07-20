using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Notifications.Commands;

public record MarkNotificationAsReadCommand : IRequest<bool>
{
    public int NotificationId { get; set; }
    public int UserId { get; set; }
}

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, bool>
{
    private readonly INotificationRepository _notificationRepository;

    public MarkNotificationAsReadCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<bool> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);

        if (notification == null)
            throw new KeyNotFoundException($"Notification with ID {request.NotificationId} not found.");

        if (notification.UserId != null && notification.UserId != request.UserId)
            throw new UnauthorizedAccessException("You don't have permission to modify this notification.");

        await _notificationRepository.MarkAsReadAsync(request.NotificationId);

        return true;
    }
}
