using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using System;
using MediatR;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using System.Text.Json.Nodes;
using HR.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace HR.Application.Attendance.ZKPython.Users.Commands;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, UserOperationResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPythonService _pythonService;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(
        IUserRepository userRepository,
        IPythonService pythonService,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _pythonService = pythonService;
        _logger = logger;
    }

    public async Task<UserOperationResult> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        return new UserOperationResult { Success = true, Message = "" };
    }
}
