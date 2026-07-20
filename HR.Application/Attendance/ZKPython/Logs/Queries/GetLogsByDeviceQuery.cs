using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using System.Text.Json.Nodes;

namespace HR.Application.Attendance.ZKPython.Logs.Queries;

public record GetLogsByDeviceQuery(int Page, int PageSize, string DeviceIp) : IRequest<GetLogsByDeviceResult>;

public class GetLogsByDeviceResult
{
    public bool Success { get; set; }
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Count { get; set; }
    public JsonObject Data { get; set; } = new();
}
