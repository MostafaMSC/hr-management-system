using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HR.Domain.Entities;
using HR.Application.Common.Interfaces;

namespace HR.Infrastructure.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        public Task<List<Device>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<Device>());
        public Task<Device?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<Device?>(null);
        public Task<Device?> GetByIpAsync(string ip, CancellationToken cancellationToken = default) => Task.FromResult<Device?>(null);
        public Task<List<Device>> GetDevicesByUserIdAsync(int userId, CancellationToken cancellationToken = default) => Task.FromResult(new List<Device>());
        public Task UpdateUserDevicesAsync(int userId, List<int> deviceIds, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
