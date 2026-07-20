using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using System;
using MediatR;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using HR.Domain.Entities;
using HR.Domain.ValueObjects;

namespace HR.Application.Attendance.ZKPython.Users.Commands;

public class EditUserCommandHandler : IRequestHandler<EditUserCommand, UserOperationResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPythonService _pythonService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ISectionRepository _sectionRepository;
    private readonly ILogger<EditUserCommandHandler> _logger;

    public EditUserCommandHandler(
        IUserRepository userRepository,
        IPythonService pythonService,
        IPasswordHasher passwordHasher,
        IDepartmentRepository departmentRepository,
        ISectionRepository sectionRepository,
        ILogger<EditUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _pythonService = pythonService;
        _passwordHasher = passwordHasher;
        _departmentRepository = departmentRepository;
        _sectionRepository = sectionRepository;
        _logger = logger;
    }

    public async Task<UserOperationResult> Handle(EditUserCommand command, CancellationToken cancellationToken)
    {
        return new UserOperationResult { Success = true, Message = "" };
    }
}
