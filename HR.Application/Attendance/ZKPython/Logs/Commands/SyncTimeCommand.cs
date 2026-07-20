using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Attendance.ZKPython.Logs.DTOs;

namespace HR.Application.Attendance.ZKPython.Logs.Commands;

public class SyncTimeCommand : IRequest<SyncLogsResult>
{
    public string DeviceIp { get; set; }

    public SyncTimeCommand(string deviceIp)
    {
        DeviceIp = deviceIp;
    }
}
