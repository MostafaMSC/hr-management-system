using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Common.Interfaces;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace HR.Application.Attendance.ZKPython.Users.Queries;

public class GetUsersFromDeviceQueryHandler : IRequestHandler<GetUsersFromDeviceQuery, GetUsersFromDeviceResult>
{
    private readonly IPythonService _pythonService;
    private readonly ILogger<GetUsersFromDeviceQueryHandler> _logger;

    public GetUsersFromDeviceQueryHandler(IPythonService pythonService, ILogger<GetUsersFromDeviceQueryHandler> logger)
    {
        _pythonService = pythonService;
        _logger = logger;
    }

    public async Task<GetUsersFromDeviceResult> Handle(GetUsersFromDeviceQuery request, CancellationToken cancellationToken)
    {
        return new GetUsersFromDeviceResult { Success = true, Users = new System.Collections.Generic.List<DeviceUserDto>() };
    }
}
