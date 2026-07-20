using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using System.Text.Json.Nodes;

namespace HR.Application.Attendance.ZKPython.Users.DTOs;

public class SyncUsersResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public JsonNode? ErrorDetail { get; set; }
}
