using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace HR.Application.Attendance.ZKPython.Users.Commands;

public class SyncUsersCommandHandler : IRequestHandler<SyncUsersCommand, SyncUsersResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPythonService _pythonService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDeviceRepository _deviceRepository;
    private readonly ILogger<SyncUsersCommandHandler> _logger;

    public SyncUsersCommandHandler(
        IUserRepository userRepository, 
        IPythonService pythonService, 
        IPasswordHasher passwordHasher,
        IDeviceRepository deviceRepository,
        ILogger<SyncUsersCommandHandler> logger)
    {
        _userRepository = userRepository;
        _pythonService = pythonService;
        _passwordHasher = passwordHasher;
        _deviceRepository = deviceRepository;
        _logger = logger;
    }

    public async Task<SyncUsersResult> Handle(SyncUsersCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SyncUsers called for DeviceIp: {DeviceIp}", request.DeviceIp);
        
        if (string.IsNullOrEmpty(request.DeviceIp))
             return new SyncUsersResult { Success = false, Message = "Device IP is required" };

        try
        {
            // Resolve Device entity first
            var device = await _deviceRepository.GetByIpAsync(request.DeviceIp, cancellationToken);
            if (device == null)
            {
                _logger.LogWarning("Device with IP {DeviceIp} not found in database. User-Device linking will be skipped.", request.DeviceIp);
            }

            var result = await _pythonService.RunPythonGetUsersAsync(request.DeviceIp, cancellationToken);

            if (result["success"]?.GetValue<bool>() == true)
            {
                var usersNode = result["users"]?.AsArray();
                if (usersNode != null)
                {
                    _logger.LogInformation("Received {Count} users from device", usersNode.Count);
                    // Pre-fetch all existing usernames to handle uniqueness in-memory
                    var existingUsernames = await _userRepository.GetAllUsernamesAsync(cancellationToken);
                    
                    var usedNames = new HashSet<string>(existingUsernames, StringComparer.OrdinalIgnoreCase);

                    foreach (var u in usersNode)
                    {
                        if (u == null) continue;

                        var userId = u["UserID"]?.ToString() ?.ToString() ?? "";
                        var name = u["Name"]?.ToString();
                        
                        _logger.LogInformation("Processing synced user from device. ID: {UserId}, Name: {Name}", userId, name);

                        if (string.IsNullOrWhiteSpace(userId)) 
                        {
                            _logger.LogWarning("Skipping user with empty UserID");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(name)) name = $"User_{userId}";

                        var card = u["Card"]?.ToString();
                        var password = u["Password"]?.ToString();
                        var privilege = u["Privilege"]?.GetValue<int>() ?? 0;
                        var roleStr = u["Role"]?.ToString(); // "User", "Administrator", etc.

                        // Determine Target Role
                        UserType targetRole = UserType.User;
                        if (privilege == 14 || roleStr == "Administrator")
                        {
                            targetRole = UserType.Administrator;
                        }

                        // Try to find user GLOBALLY by BiometricId
                        var existingUser = (await _userRepository.GetAllUsersAsync(cancellationToken)).FirstOrDefault(u => u.BiometricId == userId);
                        
                        // Determine the final unique username
                        string finalName = name;

                        // Only check for username conflict if we are creating a NEW user 
                        // OR if the existing user has a different name than what's coming in
                        // AND that new name is already taken by SOMEONE ELSE.
                        if (existingUser == null)
                        {
                            if (usedNames.Contains(finalName))
                            {
                                int suffix = 1;
                                while (usedNames.Contains(finalName))
                                {
                                    finalName = $"{name}_{suffix}";
                                    suffix++;
                                }
                            }
                        }
                        
                        if (existingUser == null)
                        {
                            var newUser = new UserInfo
                            {
                                
                                DeviceIp = request.DeviceIp, // Set primary IP
                                
                                Card = card,
                                Password = !string.IsNullOrWhiteSpace(password) ? _passwordHasher.HashPassword(password) : "",
                                Role = targetRole.ToString()
                            };
                            
                            // Link Device
                            if (device != null)
                            {
                                newUser.UserDevices ??= new List<UserDevice>();
                                newUser.UserDevices.Add(new UserDevice { DeviceId = device.Id });
                            }

                            await _userRepository.CreateAsync(newUser, cancellationToken);
                            usedNames.Add(finalName);
                        }
                        else
                        {
                            // Update existing user
                            // Consider updating Username only if it's different and not taken?
                            // For now, let's keep existing username to avoid confusion, or update if it matches logic.
                            // User wants to merge, so we just update other fields.
                            _logger.LogInformation("Updating existing user: {UserId} (Global Match)", userId);
                            
                            // Update DeviceIp to be the "last seen" IP
                            existingUser.DeviceIp = request.DeviceIp;

                            await _userRepository.UpdateAsync(existingUser, cancellationToken);
                        }
                    }
                }
                return new SyncUsersResult { Success = true, Message = "Users synced successfully" };
            }
            else
            {
                return new SyncUsersResult { Success = false, Message = "Failed to fetch users from device", ErrorDetail = result };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing users");
            return new SyncUsersResult { Success = false, Message = ex.Message, ErrorDetail = ex.InnerException?.Message };
        }
    }
}
