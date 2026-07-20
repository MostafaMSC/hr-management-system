using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Domain.Entities;

namespace HR.Application.Attendance.ZKPython.Notifications.Queries
{
    public class GetUserNotificationsQuery : IRequest<List<Notification>>
    {
        public int UserId { get; set; }
        public bool UnreadOnly { get; set; } = false;
    }
}
