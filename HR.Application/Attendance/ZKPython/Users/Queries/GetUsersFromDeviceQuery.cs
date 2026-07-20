using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using System.Collections.Generic;

namespace HR.Application.Attendance.ZKPython.Users.Queries;

public record GetUsersFromDeviceQuery(string DeviceIp) : IRequest<GetUsersFromDeviceResult>;

public class GetUsersFromDeviceResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object ErrorDetail { get; set; } = new();
    public List<DeviceUserDto> Users { get; set; } = new();
}

public class DeviceUserDto
{
    public string UserID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Card { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int? Privilege { get; set; }  // Add this - numeric privilege level
    public string Role { get; set; } = string.Empty;
    public List<DeviceHRDto> HRs { get; set; } = new();
}

public class DeviceHRDto
{
    public int FingerID { get; set; }
    public string Template { get; set; } = string.Empty;
    public int Size { get; set; }
    public bool Valid { get; set; }
}
