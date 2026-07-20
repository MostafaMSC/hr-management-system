using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Common.Interfaces;

namespace HR.Application.Attendance.ZKPython.Notifications.Commands
{
    public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, bool>
    {
        private readonly INotificationRepository _notificationRepository;

        public DeleteNotificationCommandHandler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<bool> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        {
            // Get notification to verify ownership
            var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);

            if (notification == null)
                throw new KeyNotFoundException($"Notification with ID {request.NotificationId} not found.");

            if (notification.UserId != request.UserId)
                throw new UnauthorizedAccessException("You are not authorized to delete this notification.");

            await _notificationRepository.DeleteAsync(request.NotificationId);
            return true;
        }
    }
}
