using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HR.Domain.Entities;

namespace HR.Application.Common.Interfaces
{
    public interface IDeviceRepository
    {
        Task<List<Device>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Device?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Device?> GetByIpAsync(string ip, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all devices assigned to a user
        /// </summary>
        Task<List<Device>> GetDevicesByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates the device assignments for a user
        /// </summary>
        Task UpdateUserDevicesAsync(int userId, List<int> deviceIds, CancellationToken cancellationToken = default);
    }
}
