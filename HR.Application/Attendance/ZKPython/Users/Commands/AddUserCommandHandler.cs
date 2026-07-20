using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Domain.Entities;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Domain.Enums;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using HR.Domain.ValueObjects;

namespace HR.Application.Attendance.ZKPython.Users.Commands;

public class AddUserCommandHandler : IRequestHandler<AddUserCommand, UserOperationResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPythonService _pythonService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ISectionRepository _sectionRepository;
    private readonly ILogger<AddUserCommandHandler> _logger;

    public AddUserCommandHandler(
        IUserRepository userRepository, 
        IPythonService pythonService, 
        IPasswordHasher passwordHasher,
        IDepartmentRepository departmentRepository,
        ISectionRepository sectionRepository,
        ILogger<AddUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _pythonService = pythonService;
        _passwordHasher = passwordHasher;
        _departmentRepository = departmentRepository;
        _sectionRepository = sectionRepository;
        _logger = logger;
    }

    public async Task<UserOperationResult> Handle(AddUserCommand command, CancellationToken cancellationToken)
    {
        var req = command.Request;
        _logger.LogInformation("AddUser called for Username: {Username}, DeviceIp: {DeviceIp}", req.Username, req.DeviceIp);
        
        try
        {
            // 0. Resolve Department and Section
            Department? department = null;
            if (!string.IsNullOrWhiteSpace(req.Department))
            {
                department = await _departmentRepository.GetByNameAsync(req.Department);
                if (department == null)
                {
                    department = new Department { Name = req.Department };
                    await _departmentRepository.AddAsync(department);
                }
            }

            Section? section = null;
            if (!string.IsNullOrWhiteSpace(req.Section) && department != null)
            {
                section = await _sectionRepository.GetByNameAsync(req.Section, department.Id);
                if (section == null)
                {
                    section = new Section { Name = req.Section, DepartmentId = department.Id };
                    await _sectionRepository.AddAsync(section);
                }
            }

            // Resolve PhoneNumber
            PhoneNumber? phoneNumber = null;
            if (!string.IsNullOrWhiteSpace(req.PhoneNumber))
            {
                var phoneResult = PhoneNumber.Create(req.PhoneNumber);
                if (phoneResult.IsSuccess)
                {
                    phoneNumber = phoneResult.Value;
                }
                else
                {
                     return new UserOperationResult { Success = false, Message = $"Invalid phone number: {phoneResult.Error}" };
                }
            }
            
            // 1. Add to Database First
            var newUser = new UserInfo
            {
                DeviceIp = req.DeviceIp,
                
                Email = req.Email ?? string.Empty,
                Department = department,
                Section = section,
                PhoneNumber = phoneNumber?.ToString(),
                DepartmentId = department?.Id,
                SectionId = section?.Id,
                Card = req.Card,
                Address = req.Address,
                Role = req.Role?.ToString() ?? "User",
                Gender = req.Gender?.ToString(),
                ShiftType = req.ShiftType?.ToString(),
                AccountStatus = req.AccountStatus?.ToString() ?? "Active",
                BirthDate = req.BirthDate,
                HireDate = req.HireDate,
                Is2FAEnabled = req.Is2FAEnabled,
                Password = !string.IsNullOrWhiteSpace(req.Password) ? _passwordHasher.HashPassword(req.Password) : string.Empty,
                IsSyncedToDevice = false // Not synced yet
            };

            await _userRepository.CreateAsync(newUser, cancellationToken);

            // 2. Try to Sync to Device via Python
            JsonNode result;
            try
            {
                result = await _pythonService.RunPythonAddUserAsync(req.DeviceIp, req.Username, cancellationToken);
            }
            catch (Exception pyEx)
            {
                _logger.LogWarning("âŒ Exception when reaching Python device sync for AddUser: {Message}", pyEx.Message);
                return new UserOperationResult { 
                    Success = true, 
                    Message = "ØªÙ… Ø¥Ø¶Ø§ÙØ© Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù… ÙÙŠ Ø§Ù„Ù†Ø¸Ø§Ù… Ø¨Ù†Ø¬Ø§Ø­ØŒ Ù„ÙƒÙ† ÙØ´Ù„ Ø§Ù„Ø§ØªØµØ§Ù„ Ø¨Ø¬Ù‡Ø§Ø² Ø§Ù„Ø¨ØµÙ…Ø© (Ø³ÙŠÙ„Ø²Ù… Ù…Ø²Ø§Ù…Ù†ØªÙ‡ Ù„Ø§Ø­Ù‚Ù‹Ø§).", 
                    User = newUser 
                };
            }

            if (result["success"]?.GetValue<bool>() == true)
            {
                await _userRepository.CreateAsync(newUser, cancellationToken);

                return new UserOperationResult { Success = true, Message = "ØªÙ… Ø¥Ø¶Ø§ÙØ© Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù… ÙÙŠ Ø§Ù„Ù†Ø¸Ø§Ù… ÙˆÙ…Ø²Ø§Ù…Ù†ØªÙ‡ Ù…Ø¹ Ø¬Ù‡Ø§Ø² Ø§Ù„Ø¨ØµÙ…Ø© Ø¨Ù†Ø¬Ø§Ø­.", User = newUser };
            }
            else
            {
                 var errorMsg = result["error"]?.ToString() ?? "Unknown error";
                 _logger.LogWarning("âŒ Failed to add user to device. DeviceIp: {DeviceIp}, Error: {Error}", req.DeviceIp, errorMsg);
                 
                 return new UserOperationResult { 
                     Success = true, 
                     Message = "ØªÙ… Ø¥Ø¶Ø§ÙØ© Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù… ÙÙŠ Ø§Ù„Ù†Ø¸Ø§Ù… ÙÙ‚Ø· (Ù„Ù… ØªØªÙ… Ù…Ø²Ø§Ù…Ù†ØªÙ‡ Ù…Ø¹ Ø¬Ù‡Ø§Ø² Ø§Ù„Ø¨ØµÙ…Ø© Ø¨Ø³Ø¨Ø¨ Ù…Ø´ÙƒÙ„Ø© ÙÙŠ Ø§Ù„Ø§ØªØµØ§Ù„).", 
                     User = newUser,
                     ErrorDetail = result 
                 };
            }
        }
        catch (Exception ex)
        {
             return new UserOperationResult { Success = false, Message = ex.Message };
        }
    }
}
