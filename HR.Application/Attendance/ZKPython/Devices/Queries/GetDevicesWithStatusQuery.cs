using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Linq;

namespace HR.Application.Attendance.ZKPython.Devices.Queries
{
    public record DeviceStatusDto(string Ip, string Name, bool IsConnected);

    public record GetDevicesWithStatusQuery() : IRequest<List<DeviceStatusDto>>;

    public class GetDevicesWithStatusQueryHandler : IRequestHandler<GetDevicesWithStatusQuery, List<DeviceStatusDto>>
    {
        private readonly IDeviceRepository _deviceRepository;

        public GetDevicesWithStatusQueryHandler(IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public async Task<List<DeviceStatusDto>> Handle(GetDevicesWithStatusQuery request, CancellationToken cancellationToken)
        {
            var devices = await _deviceRepository.GetAllAsync(cancellationToken);

            var tasks = devices.Select(async d =>
            {
                bool online = await IsPortOpen(d.IpAddress);
                return new DeviceStatusDto(d.IpAddress, d.Name, online);
            });

            return (await Task.WhenAll(tasks)).ToList();
        }

        private async Task<bool> IsPortOpen(string host, int port = 4370)
        {
            try
            {
                using var client = new TcpClient();
                var task = client.ConnectAsync(host, port);
                if (await Task.WhenAny(task, Task.Delay(1500)) == task)
                {
                    return client.Connected;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
