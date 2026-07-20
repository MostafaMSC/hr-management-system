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

    public UpdateUserDevicesCommandHandler(
        IDeviceRepository deviceRepository,
        IUserRepository userRepository,
        IDeviceProviderFactory deviceProviderFactory,
        IFingerprintRepository fingerprintRepository,
        ILogger<UpdateUserDevicesCommandHandler> logger,
        ICacheService cache)
    {
        _deviceRepository = deviceRepository;
        _userRepository = userRepository;
        _deviceProviderFactory = deviceProviderFactory;
        _fingerprintRepository = fingerprintRepository;
        _logger = logger;
        _cache = cache;
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

            // Fetch fingerprints for the user
            var userFingerprints = (await _fingerprintRepository.GetByUserIdAsync(user.Id, cancellationToken)).ToList();

            // 2. Prepare Lists
            var allDevices = await _deviceRepository.GetAllAsync(cancellationToken);
            var currentDevices = await _deviceRepository.GetDevicesByUserIdAsync(request.UserId, cancellationToken);
            var currentDeviceIds = currentDevices.Select(d => d.Id).ToHashSet();
            var requestedDeviceIds = request.DeviceIds.ToHashSet();

            // Devices to interact with
            var devicesToRemove = allDevices.Where(d => currentDeviceIds.Contains(d.Id) && !requestedDeviceIds.Contains(d.Id)).ToList();
            var devicesToSync = allDevices.Where(d => requestedDeviceIds.Contains(d.Id)).ToList();

            var results = new List<string>();
            var errors = new List<string>();
            var successfulSyncDeviceIds = new List<int>();

            // Convert Fingerprints to DTOs for the provider
            var fingerprintDtos = userFingerprints.Select(f => new DeviceFingerprintDto
            {
                FingerIndex = f.FingerIndex,
                Template = Convert.ToBase64String(f.Template)
            }).ToList();

            var syncUserDto = new SyncUserRequestDto
            {
                UserId = user.BiometricId,
                Name = user.Username ?? $"{user.FirstName} {user.LastName}",
                Password = user.Password,
                Card = user.Card ?? "0",
                Privilege = user.Role == UserType.Administrator ? 14 : 0,
                Fingerprints = fingerprintDtos
            };

            // =========================================================================================
            // PHASE 1: Sync to Target Devices (Parallel Safe Writes)
            // =========================================================================================
            _logger.LogInformation("🚀 Starting Phase 1: Syncing user {UserId} to {Count} devices...", user.BiometricId, devicesToSync.Count);

            var syncTasks = devicesToSync.Select(async device =>
            {
                try
                {
                    _logger.LogInformation("⏳ Syncing to {DeviceName} ({Ip})...", device.Name, device.IpAddress);
                    var provider = _deviceProviderFactory.GetProvider(device.Protocol);

                    var success = await provider.SyncFullUserAsync(device.IpAddress, syncUserDto, cancellationToken);

                    if (success)
                    {
                        return (DeviceId: device.Id, DeviceName: device.Name, Success: true, Message: $"تمت المزامنة مع {device.Name} بنجاح");
                    }
                    else
                    {
                        return (DeviceId: device.Id, DeviceName: device.Name, Success: false, Message: $"فشل المزامنة مع {device.Name}");
                    }
                }
                catch (Exception ex)
                {
                    return (DeviceId: device.Id, DeviceName: device.Name, Success: false, Message: $"خطأ في {device.Name}: {ex.Message}");
                }
            });

            var syncResults = await Task.WhenAll(syncTasks);

            foreach (var res in syncResults)
            {
                if (res.Success)
                {
                    successfulSyncDeviceIds.Add(res.DeviceId);
                    results.Add(res.Message);
                }
                else
                {
                    errors.Add(res.Message);
                    _logger.LogWarning("❌ Sync failed for {Device}: {Message}", res.DeviceName, res.Message);
                }
            }

            // =========================================================================================
            // PHASE 2: Delete from Old Devices (Safe Deletes)
            // =========================================================================================
            bool isSafeToDelete = successfulSyncDeviceIds.Count > 0 || requestedDeviceIds.Count == 0;

            if (isSafeToDelete && devicesToRemove.Count > 0)
            {
                _logger.LogInformation("🚀 Starting Phase 2: Removing from {Count} old devices...", devicesToRemove.Count);

                var removeTasks = devicesToRemove.Select(async device =>
                {
                    try
                    {
                        var provider = _deviceProviderFactory.GetProvider(device.Protocol);
                        var success = await provider.DeleteUserAsync(device.IpAddress, user.BiometricId, cancellationToken);

                        if (success)
                            return $"تم الحذف من {device.Name}";
                        else
                            return $"فشل الحذف من {device.Name}";
                    }
                    catch (Exception ex)
                    {
                        return $"خطأ أثناء الحذف من {device.Name}: {ex.Message}";
                    }
                });

                var removeResults = await Task.WhenAll(removeTasks);
                results.AddRange(removeResults);
            }
            else if (devicesToRemove.Count > 0)
            {
                var msg = "⚠️ تم إيقاف الحذف من الأجهزة القديمة لأن جميع عمليات النقل للأجهزة الجديدة فشلت. بيانات المستخدم آمنة في الأجهزة القديمة.";
                _logger.LogWarning(msg);
                errors.Add(msg);
            }

            // =========================================================================================
            // PHASE 3: Update Database
            // =========================================================================================
            var finalDbDeviceIds = new HashSet<int>(successfulSyncDeviceIds);

            if (!isSafeToDelete)
            {
                // If we skipped deleting, we MUST keep the old devices in the DB
                foreach (var d in devicesToRemove) finalDbDeviceIds.Add(d.Id);
            }

            await _deviceRepository.UpdateUserDevicesAsync(request.UserId, finalDbDeviceIds.ToList(), cancellationToken);

            // Invalidate device cache
            await _cache.RemoveAsync("Cache_AllDevices", cancellationToken);

            var isOverallSuccess = errors.Count == 0 || successfulSyncDeviceIds.Count > 0;

            return new UpdateUserDevicesResult
            {
                Success = isOverallSuccess,
                Message = isOverallSuccess ? "تم تحديث الأجهزة" : "فشل جميع العمليات",
                DevicesAdded = successfulSyncDeviceIds.Count,
                DevicesRemoved = devicesToRemove.Count,
                Details = results,
                Errors = errors
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
