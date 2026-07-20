using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;

namespace HR.Application.Attendance.ZKPython.UserDevices.Commands;

public class EnrollUserCommandHandler : IRequestHandler<EnrollUserCommand, UserOperationResult>
{
    private readonly IPythonService _pythonService;
    private readonly ILogger<EnrollUserCommandHandler> _logger;

    public EnrollUserCommandHandler(IPythonService pythonService, ILogger<EnrollUserCommandHandler> logger)
    {
        _pythonService = pythonService;
        _logger = logger;
    }

    public async Task<UserOperationResult> Handle(EnrollUserCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("EnrollUser Called for UserId: {UserId} on Device: {DeviceIp}", command.UserId, command.DeviceIp);

        try
        {
            var result = await _pythonService.RunPythonEnrollUserAsync(command.DeviceIp, command.UserId, command.FingerId, cancellationToken);

            return new UserOperationResult
            {
                Success = result.Success,
                Message = result.Message,
                ErrorDetail = result.Success ? null : result.Data
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Enrollment failed for UserId: {UserId}", command.UserId);
            return new UserOperationResult { Success = false, Message = ex.Message };
        }
    }
}
