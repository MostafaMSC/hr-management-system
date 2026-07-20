using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using System.Text.Json.Nodes;

namespace HR.Application.Attendance.ZKPython.Logs.DTOs;

public class SyncLogsResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Added { get; set; }
    public int Skipped { get; set; }
    public int Total { get; set; }
    public JsonNode? ErrorDetail { get; set; }
}
