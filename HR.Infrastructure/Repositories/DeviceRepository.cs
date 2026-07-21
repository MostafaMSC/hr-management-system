using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HR.Domain.Entities;
using HR.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HR.Infrastructure.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly IApplicationDbContext _context;

        public DeviceRepository(IApplicationDbContext context)
        {
            _context = context;
        }

        public Task<List<Device>> GetAllAsync(CancellationToken cancellationToken = default) => 
            _context.Devices.ToListAsync(cancellationToken);
            
        public Task<Device?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => 
            _context.Devices.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            
        public Task<Device?> GetByIpAsync(string ip, CancellationToken cancellationToken = default) => 
            _context.Devices.FirstOrDefaultAsync(d => d.IpAddress == ip, cancellationToken);
            
        public Task<List<Device>> GetDevicesByUserIdAsync(int userId, CancellationToken cancellationToken = default) => 
            _context.UserDevices
                .Where(ud => ud.UserInfoId == userId)
                .Select(ud => ud.Device)
                .ToListAsync(cancellationToken);
                
        public async Task UpdateUserDevicesAsync(int userId, List<int> deviceIds, CancellationToken cancellationToken = default)
        {
            var existing = await _context.UserDevices.Where(ud => ud.UserInfoId == userId).ToListAsync(cancellationToken);
            _context.UserDevices.RemoveRange(existing);
            
            foreach (var deviceId in deviceIds)
            {
                _context.UserDevices.Add(new UserDevice { UserInfoId = userId, DeviceId = deviceId });
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
