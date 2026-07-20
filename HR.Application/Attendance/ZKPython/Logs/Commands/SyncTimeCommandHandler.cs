using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Attendance.ZKPython.Logs.DTOs;
using HR.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;

namespace HR.Application.Attendance.ZKPython.Logs.Commands;

public class SyncTimeCommandHandler : IRequestHandler<SyncTimeCommand, SyncLogsResult>
{
    private readonly IPythonService _pythonService;
    private readonly ILogger<SyncTimeCommandHandler> _logger;

    public SyncTimeCommandHandler(IPythonService pythonService, ILogger<SyncTimeCommandHandler> logger)
    {
        _pythonService = pythonService;
        _logger = logger;
    }

    public async Task<SyncLogsResult> Handle(SyncTimeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.DeviceIp))
                return new SyncLogsResult { Success = false, Message = "Device IP is required" };

            var result = await _pythonService.RunPythonSyncTimeAsync(request.DeviceIp, cancellationToken);

            if (result["success"]?.GetValue<bool>() == true)
            {
                var msg = result["message"]?.ToString();
                var syncedTime = result["synced_time"]?.ToString();
                return new SyncLogsResult
                {
                    Success = true,
                    Message = $"{msg}. (Server Time: {syncedTime})"
                };
            }

            return new SyncLogsResult
            {
                Success = false,
                Message = "Failed to sync time",
                ErrorDetail = result["error"]?.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing time");
            return new SyncLogsResult { Success = false, Message = ex.Message };
        }
    }
}
