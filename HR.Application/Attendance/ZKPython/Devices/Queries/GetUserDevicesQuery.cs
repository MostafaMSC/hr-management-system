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

namespace HR.Application.Attendance.ZKPython.Devices.Queries
{
    public record GetUserDevicesQuery(int UserId) : IRequest<List<Device>>;

    public class GetUserDevicesQueryHandler : IRequestHandler<GetUserDevicesQuery, List<Device>>
    {
        private readonly IDeviceRepository _deviceRepository;

        public GetUserDevicesQueryHandler(IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public async Task<List<Device>> Handle(GetUserDevicesQuery request, CancellationToken cancellationToken)
        {
            return await _deviceRepository.GetDevicesByUserIdAsync(request.UserId, cancellationToken);
        }
    }
}
