using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Common.Interfaces;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Attendance.ZKPython.Users.DTOs;
using HR.Application.Attendance.ZKPython.Tickets.DTOs;
using MediatR;
using HR.Domain.Entities;
using System.Collections.Generic;
using HR.Application.Common.Interfaces;
using HR.Application.Common;

namespace HR.Application.Attendance.ZKPython.Devices.Queries
{
    public record GetAllDevicesQuery() : IRequest<List<Device>>;

    public class GetAllDevicesQueryHandler : IRequestHandler<GetAllDevicesQuery, List<Device>>
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly ICacheService _cache;

        public GetAllDevicesQueryHandler(IDeviceRepository deviceRepository, ICacheService cache)
        {
            _deviceRepository = deviceRepository;
            _cache = cache;
        }

        public async Task<List<Device>> Handle(GetAllDevicesQuery request, CancellationToken cancellationToken)
        {
            // Try cache first
            var cached = await _cache.GetAsync<List<Device>>("Cache_AllDevices", cancellationToken);
            if (cached is not null) return cached;

            // Cache miss â€“ fetch from DB and cache
            var devices = await _deviceRepository.GetAllAsync(cancellationToken);
            await _cache.SetAsync("Cache_AllDevices", devices, TimeSpan.FromMinutes(5), cancellationToken);
            return devices;
        }
    }
}
