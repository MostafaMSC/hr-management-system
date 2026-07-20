using HR.Application.Common.Interfaces;
using HR.Application.Notifications.DTOs;
using MediatR;

namespace HR.Application.Notifications.Queries;

public record GetUserNotificationsQuery(int UserId, bool UnreadOnly = false, int? PageNumber = null, int? PageSize = null) : IRequest<List<NotificationDto>>;

public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, List<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetUserNotificationsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<List<NotificationDto>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetUserNotificationsAsync(request.UserId, request.UnreadOnly);

        // Optional pagination logic
        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            var skip = (request.PageNumber.Value - 1) * request.PageSize.Value;
            notifications = notifications.Skip(skip).Take(request.PageSize.Value).ToList();
        }

        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            Type = n.Type,
            Link = n.Link,
            Data = n.Data,
            UserId = n.UserId,
            CreatedAt = n.CreatedAt
        }).ToList();
    }
}
