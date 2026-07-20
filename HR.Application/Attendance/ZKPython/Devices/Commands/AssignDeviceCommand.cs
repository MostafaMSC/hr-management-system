using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Domain.Entities;

namespace HR.Application.Attendance.ZKPython.Devices.Commands
{
    public record AssignDeviceCommand(int UserId, int DeviceId) : IRequest<AssignDeviceResult>;

    public class AssignDeviceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
