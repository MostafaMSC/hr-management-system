using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Domain.Entities;
using HR.Domain.Exceptions;
using HR.Application.Attendance.ZKPython.Logs.DTOs;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Domain.Entities;
using HR.Domain.Exceptions;
using HR.Application.Attendance.ZKPython.Logs.DTOs;
using HR.Application.Common.Interfaces;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;


namespace HR.Application.Attendance.ZKPython.Logs.Commands;

public class SyncLogsCommandHandler : IRequestHandler<SyncLogsCommand, SyncLogsResult>
{
    private readonly IAttendanceLogRepository _logRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IDeviceProviderFactory _providerFactory;
    private readonly ILogger<SyncLogsCommandHandler> _logger;

    public SyncLogsCommandHandler(
        IAttendanceLogRepository logRepository,
        IDeviceRepository deviceRepository,
        IDeviceProviderFactory providerFactory,
        ILogger<SyncLogsCommandHandler> logger)
    {
        _logRepository = logRepository;
        _deviceRepository = deviceRepository;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public async Task<SyncLogsResult> Handle(SyncLogsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var device = await _deviceRepository.GetByIpAsync(request.DeviceIp);
            if (device == null)
            {
                return new SyncLogsResult { Success = false, ErrorDetail = "Device not found." };
            }

            var provider = _providerFactory.GetProvider(device.Protocol);
            var logs = await provider.FetchLogsAsync(device.IpAddress, cancellationToken);

            // Pending saving logic
            return new SyncLogsResult { Success = true, Message = $"Fetched {logs.Count} logs." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync failed.");
            return new SyncLogsResult { Success = false, ErrorDetail = ex.Message };
        }
    }
}
