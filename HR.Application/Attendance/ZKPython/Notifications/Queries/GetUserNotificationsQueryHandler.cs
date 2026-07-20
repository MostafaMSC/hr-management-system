using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Domain.Entities;
using HR.Application.Common.Interfaces;

namespace HR.Application.Attendance.ZKPython.Notifications.Queries
{
    public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, List<Notification>>
    {
        private readonly INotificationRepository _notificationRepository;

        public GetUserNotificationsQueryHandler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<List<Notification>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        {
            return await _notificationRepository.GetUserNotificationsAsync(
                request.UserId,
                request.UnreadOnly);
        }
    }
}
