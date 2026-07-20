using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

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
    public object Details { get; set; } = null!;
    public object Errors { get; set; } = null!;
}

public class UpdateUserDevicesCommandHandler : IRequestHandler<UpdateUserDevicesCommand, UpdateUserDevicesResult>
{
    public async Task<UpdateUserDevicesResult> Handle(UpdateUserDevicesCommand request, CancellationToken cancellationToken)
    {
        return new UpdateUserDevicesResult { Success = true, Message = "" };
    }
}
