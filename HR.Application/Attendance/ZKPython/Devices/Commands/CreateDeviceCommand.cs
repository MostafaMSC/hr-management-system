using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HR.Application.Attendance.ZKPython.Devices.Commands;

public class CreateDeviceCommand : IRequest<int>
{
    public string DeviceName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 4370;
    public string? SerialNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DeviceProtocol Protocol { get; set; } = DeviceProtocol.ZkTecoTcp;
}

public class CreateDeviceCommandHandler : IRequestHandler<CreateDeviceCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateDeviceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = new Device
        {
            DeviceName = request.DeviceName,
            IpAddress = request.IpAddress,
            Port = request.Port,
            SerialNumber = request.SerialNumber,
            IsActive = request.IsActive,
            Protocol = request.Protocol
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync(cancellationToken);

        return device.Id;
    }
}
