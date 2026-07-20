using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Notifications.Commands;

public record MarkAllNotificationsAsReadCommand : IRequest<bool>
{
    public int UserId { get; set; }
}

public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, bool>
{
    private readonly INotificationRepository _notificationRepository;

    public MarkAllNotificationsAsReadCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<bool> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        await _notificationRepository.MarkAllAsReadAsync(request.UserId);
        return true;
    }
}
