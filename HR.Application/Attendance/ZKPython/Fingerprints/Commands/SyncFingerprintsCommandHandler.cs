using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Exceptions;

namespace HR.Application.Attendance.ZKPython.UserDevices.Commands;

public class SyncHRsCommandHandler : IRequestHandler<SyncHRsCommand, SyncHRsResult>
{
    private readonly IFingerprintRepository _HRRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDeviceRepository _deviceRepository;
    private readonly IPythonService _pythonService;
    private readonly ILogger<SyncHRsCommandHandler> _logger;

    public SyncHRsCommandHandler(
        IFingerprintRepository HRRepository,
        IUserRepository userRepository,
        IDeviceRepository deviceRepository,
        IPythonService pythonService,
        ILogger<SyncHRsCommandHandler> logger)
    {
        _HRRepository = HRRepository;
        _userRepository = userRepository;
        _deviceRepository = deviceRepository;
        _pythonService = pythonService;
        _logger = logger;
    }

    public async Task<SyncHRsResult> Handle(SyncHRsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ðŸ”„ Starting HR sync for DeviceIp: {DeviceIp}", request.DeviceIp ?? "All Devices");

        try
        {
            var devices = await GetDevicesAsync(request.DeviceIp, cancellationToken);

            int totalAdded = 0, totalUpdated = 0, totalSkipped = 0;
            var errors = new List<string>();

            foreach (var deviceIp in devices)
            {
                try
                {
                    _logger.LogInformation("ðŸ“¡ Syncing HRs from device: {DeviceIp}", deviceIp);

                    // 1. Get templates from Python Service
                    var jsonResult = await _pythonService.RunPythonGetTemplatesAsync(deviceIp, cancellationToken);

                    // Deserialize to PythonTemplateResult
                    var pythonResult = jsonResult.Deserialize<PythonTemplateResult>(new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (pythonResult == null || !pythonResult.Success)
                    {
                        var errorMsg = $"Device {deviceIp}: {pythonResult?.Error ?? "Unknown error or null result"}";
                        _logger.LogWarning("âš ï¸ {Error}", errorMsg);
                        errors.Add(errorMsg);
                        continue;
                    }

                    _logger.LogInformation("ðŸ“‹ Python returned {Count} templates from {DeviceIp}",
                        pythonResult.Count, deviceIp);

                    // 2. Prepare User Mapping
                    // ðŸ”¥ CRITICAL: Map BiometricId (string) to UserInfo.Id (int)
                    var BiometricIds = pythonResult.Templates
                        .Select(t => t.UserId)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Distinct()
                        .ToList();

                    // Get user mapping from Repository
                    // We need a way to get users by a list of BiometricIds. 
                    // IUserRepository doesn't have a bulk lookup by BiometricId list.
                    // We can iterate or fetch all users (if not too many) or add a new method.
                    // For now, let's fetch all users, or optimize if needed. 
                    // Given the repository capabilities, fetching all might be safest if no bulk method exists.
                    // But wait, `UserRepository` likely exposes existing DbSet in Infrastructure, but Abstracted here.
                    // Let's use `GetAllUsersAsync` or check if we can add `GetUsersByBiometricIds`.
                    // To avoid editing Interface again right now, I'll fetch *All* users if list is large, or iterate if small.
                    // Or... I can search specifically.
                    // "GetByBiometricIdAsync" exists (single).

                    // Optimization: Get ALL users and build dictionary in memory. 
                    // This is acceptable for < 10k users.
                    var allUsers = await _userRepository.GetAllUsersAsync(cancellationToken);

                    var userMap = allUsers
                        .Where(u => !string.IsNullOrEmpty(u.BiometricId))
                        .GroupBy(u => u.BiometricId!)
                        .ToDictionary(g => g.Key.Trim(), g => g.First(), StringComparer.OrdinalIgnoreCase);

                    _logger.LogInformation("ðŸ‘¥ Loaded {Count} users for mapping", userMap.Count);

                    var skippedUids = new List<string>();
                    var processedKeys = new HashSet<string>();

                    foreach (var template in pythonResult.Templates)
                    {
                        if (string.IsNullOrWhiteSpace(template.UserId)) continue;
                        var cleanUserId = template.UserId.Trim();

                        // Prevent processing duplicates from device output
                        var uniqueKey = $"{cleanUserId}_{template.Fid}";
                        if (processedKeys.Contains(uniqueKey)) continue;
                        processedKeys.Add(uniqueKey);

                        try
                        {
                            if (!userMap.TryGetValue(cleanUserId, out var userInfo))
                            {
                                _logger.LogWarning("âš ï¸ User with BiometricId '{BiometricId}' not found in database. Skipping HR UID:{Uid}, FID:{Fid}",
                                    cleanUserId, template.Uid, template.Fid);
                                skippedUids.Add($"UID:{template.Uid}(BiometricId:{cleanUserId})");
                                totalSkipped++;
                                continue;
                            }

                            var templateBytes = Convert.FromHexString(template.Template);

                            // Check existence using Repository
                            var existing = await _HRRepository.GetByUserIdAndFingerIndexAsync(userInfo.Id, template.Fid, cancellationToken);

                            if (existing != null)
                            {
                                // Update existing HR
                                existing.Template = templateBytes;
                                existing.TemplateSize = template.Size;
                                existing.IsValid = template.Valid == 1;

                                existing.UserId = userInfo.Id; // âœ… Correct FK

                                existing.UpdatedAt = DateTime.UtcNow;

                                await _HRRepository.UpdateAsync(existing, cancellationToken);
                                totalUpdated++;

                                _logger.LogDebug("ðŸ”„ Updated HR for User {Username} (Id:{UserId}, BiometricId:{BiometricId}), Finger {FingerIndex}",
                                    userInfo.Username, userInfo.Id, cleanUserId, template.Fid);
                            }
                            else
                            {
                                // Add new HR
                                var newHR = new HR.Domain.Entities.Fingerprint
                                {
                                    UserId = userInfo.Id,


                                    FingerIndex = template.Fid,
                                    Template = templateBytes,
                                    TemplateSize = template.Size,
                                    DeviceIp = deviceIp,
                                    IsValid = template.Valid == 1,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };

                                await _HRRepository.AddAsync(newHR, cancellationToken);
                                totalAdded++;

                                _logger.LogDebug("âž• Added HR for User {Username} (Id:{UserId}, BiometricId:{BiometricId}), Finger {FingerIndex}",
                                    userInfo.Username, userInfo.Id, cleanUserId, template.Fid);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "âŒ Error processing HR UID {Uid}, FID {Fid}, BiometricId {BiometricId}",
                                template.Uid, template.Fid, template.UserId);
                            totalSkipped++;
                        }
                    }

                    if (skippedUids.Any())
                    {
                        var distinctSkipped = skippedUids.Distinct().Take(10).ToList();
                        var skippedMsg = $"Skipped {skippedUids.Count} HRs (First {distinctSkipped.Count}): {string.Join(", ", distinctSkipped)}";
                        if (skippedUids.Distinct().Count() > 10) skippedMsg += "... and more";

                        _logger.LogWarning("âš ï¸ {SkippedMsg} from device {DeviceIp}. Reason: Users not found in database",
                            skippedMsg, deviceIp);
                        errors.Add(skippedMsg);
                    }

                    // Save changes for this device
                    await _HRRepository.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("ðŸ’¾ Saved {Added} new, {Updated} updated HRs for device {DeviceIp}",
                        totalAdded, totalUpdated, deviceIp);
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Device {deviceIp}: {ex.Message}";
                    _logger.LogError(ex, "âŒ Error syncing HRs from {DeviceIp}", deviceIp);
                    errors.Add(errorMsg);
                }
            }

            var message = $"ØªÙ…Øª Ù…Ø²Ø§Ù…Ù†Ø© {totalAdded + totalUpdated} Ø¨ØµÙ…Ø© ({totalAdded} Ø¬Ø¯ÙŠØ¯Ø©ØŒ {totalUpdated} Ù…Ø­Ø¯Ø«Ø©ØŒ {totalSkipped} Ù…ØªØ¬Ø§ÙˆØ²Ø©)";

            if (errors.Any())
            {
                message += $". Ø£Ø®Ø·Ø§Ø¡: {errors.Count}";
            }

            if (totalSkipped > 0)
            {
                var skippedDetails = $"ØªÙ… ØªØ¬Ø§ÙˆØ² {totalSkipped} Ø¨ØµÙ…Ø© Ù„Ø¹Ø¯Ù… Ø§Ù„Ø¹Ø«ÙˆØ± Ø¹Ù„Ù‰ Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù…ÙŠÙ† ÙÙŠ Ù‚Ø§Ø¹Ø¯Ø© Ø§Ù„Ø¨ÙŠØ§Ù†Ø§Øª.";
                message += $"\n{skippedDetails}";
            }

            _logger.LogInformation("âœ… HR sync completed: {Message}", message);

            return new SyncHRsResult
            {
                Success = true,
                Message = message,
                Added = totalAdded,
                Updated = totalUpdated,
                Skipped = totalSkipped,
                Total = totalAdded + totalUpdated + totalSkipped,
                ErrorDetail = errors.Any() ? string.Join("; ", errors) :
                             (totalSkipped > 0 ? "Some users missing" : null)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "â Œ Fatal error during HR sync");
            throw;
        }
    }

    private async Task<List<string>> GetDevicesAsync(string? specificDeviceIp, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(specificDeviceIp))
        {
            return new List<string> { specificDeviceIp };
        }

        var devices = await _deviceRepository.GetAllAsync(cancellationToken);
        var ips = devices
            .Where(d => !string.IsNullOrEmpty(d.IpAddress))
            .Select(d => d.IpAddress)
            .ToList();

        if (!ips.Any())
        {
            // Fallback? Or throw?
            // If repository is empty, maybe nothing has been seeded yet.
            // But usually we expect devices.
            throw new Exception("Ù„Ù… ÙŠØªÙ… Ø§Ù„Ø¹Ø«ÙˆØ± Ø¹Ù„Ù‰ Ø£Ø¬Ù‡Ø²Ø© ÙÙŠ Ø§Ù„Ù†Ø¸Ø§Ù…");
        }

        return ips!;
    }
}
