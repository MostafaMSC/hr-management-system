using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;

namespace HR.Application.Attendance.ZKPython.Notifications.Queries
{
    public class GetUnreadCountQuery : IRequest<int>
    {
        public int UserId { get; set; }
    }
}
