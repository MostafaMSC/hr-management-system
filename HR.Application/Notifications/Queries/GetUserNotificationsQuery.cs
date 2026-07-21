using HR.Application.Common.Interfaces;
using HR.Application.Notifications.DTOs;
using MediatR;

using HR.Application.Common.Models;

namespace HR.Application.Notifications.Queries;

public record GetUserNotificationsQuery(int UserId, bool UnreadOnly = false, int PageNumber = 1, int PageSize = 10) : IRequest<PaginatedResult<NotificationDto>>;

public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, PaginatedResult<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetUserNotificationsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<PaginatedResult<NotificationDto>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _notificationRepository.GetUserNotificationsAsync(request.UserId, request.UnreadOnly);
        
        var totalCount = notifications.Count();
        
        var data = notifications
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var resultData = data.Select(n => new NotificationDto
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

        return new PaginatedResult<NotificationDto>(resultData, totalCount, request.PageNumber, request.PageSize);
    }
}
