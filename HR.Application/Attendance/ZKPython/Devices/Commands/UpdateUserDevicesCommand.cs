using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace HR.Application.Attendance.ZKPython.Devices.Commands;

public class UpdateUserDevicesCommand : IRequest<UpdateUserDevicesResult>
{
    public int UserId { get; set; }
    public List<int> DeviceIds { get; set; } = new();
}

public class UpdateUserDevicesResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DevicesAdded { get; set; }
    public int DevicesRemoved { get; set; }
    public List<string> Details { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class UpdateUserDevicesCommandHandler : IRequestHandler<UpdateUserDevicesCommand, UpdateUserDevicesResult>
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDeviceProviderFactory _deviceProviderFactory;
    private readonly IFingerprintRepository _fingerprintRepository;
    private readonly ILogger<UpdateUserDevicesCommandHandler> _logger;
    private readonly ICacheService _cache;
    private readonly IApplicationDbContext _dbContext;

    public UpdateUserDevicesCommandHandler(
        IDeviceRepository deviceRepository,
        IUserRepository userRepository,
        IDeviceProviderFactory deviceProviderFactory,
        IFingerprintRepository fingerprintRepository,
        ILogger<UpdateUserDevicesCommandHandler> logger,
        ICacheService cache,
        IApplicationDbContext dbContext)
    {
        _deviceRepository = deviceRepository;
        _userRepository = userRepository;
        _deviceProviderFactory = deviceProviderFactory;
        _fingerprintRepository = fingerprintRepository;
        _logger = logger;
        _cache = cache;
        _dbContext = dbContext;
    }

    public async Task<UpdateUserDevicesResult> Handle(UpdateUserDevicesCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Get user
            var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return new UpdateUserDevicesResult { Success = false, Message = "المستخدم غير موجود" };
            }

            if (string.IsNullOrEmpty(user.BiometricId))
            {
                // Assign a Device User ID (BiometricId) if missing
                user.BiometricId = request.UserId.ToString();
                await _userRepository.UpdateAsync(user, cancellationToken);
            }

            // 2. Prepare Lists
            var allDevices = await _deviceRepository.GetAllAsync(cancellationToken);
            var currentDevices = await _deviceRepository.GetDevicesByUserIdAsync(request.UserId, cancellationToken);
            var currentDeviceIds = currentDevices.Select(d => d.Id).ToHashSet();
            var requestedDeviceIds = request.DeviceIds.ToHashSet();

            // Devices to interact with
            var devicesToRemove = allDevices.Where(d => currentDeviceIds.Contains(d.Id) && !requestedDeviceIds.Contains(d.Id)).ToList();
            var devicesToSync = allDevices.Where(d => requestedDeviceIds.Contains(d.Id)).ToList();

            // 3. Update Database (Desired State)
            await _deviceRepository.UpdateUserDevicesAsync(request.UserId, request.DeviceIds, cancellationToken);

            // 4. Queue Sync Tasks
            foreach (var device in devicesToSync)
            {
                _dbContext.DeviceSyncTasks.Add(new DeviceSyncTask
                {
                    UserId = request.UserId,
                    DeviceId = device.Id,
                    Action = SyncAction.Add,
                    Status = SyncTaskStatus.Pending
                });
            }

            foreach (var device in devicesToRemove)
            {
                _dbContext.DeviceSyncTasks.Add(new DeviceSyncTask
                {
                    UserId = request.UserId,
                    DeviceId = device.Id,
                    Action = SyncAction.Delete,
                    Status = SyncTaskStatus.Pending
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Invalidate device cache
            await _cache.RemoveAsync("Cache_AllDevices", cancellationToken);

            return new UpdateUserDevicesResult
            {
                Success = true,
                Message = "تم حفظ الأجهزة. سيتم مزامنة الأجهزة في الخلفية.",
                DevicesAdded = devicesToSync.Count,
                DevicesRemoved = devicesToRemove.Count,
                Details = new List<string> { "Tasks queued for background sync." },
                Errors = new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🔴 Error in UpdateUserDevicesCommandHandler");
            return new UpdateUserDevicesResult
            {
                Success = false,
                Message = $"خطأ داخلي: {ex.Message}"
            };
        }
    }
}
