using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Notifications.Queries;

public record GetUnreadCountQuery(int UserId) : IRequest<int>;

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly INotificationRepository _notificationRepository;

    public GetUnreadCountQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        return await _notificationRepository.GetUnreadCountAsync(request.UserId);
    }
}
