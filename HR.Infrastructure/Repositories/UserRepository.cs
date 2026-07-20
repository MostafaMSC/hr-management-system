using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Attendance.ZKPython.DTOs;
using HR.Application.Common.Interfaces;

namespace HR.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        public Task<UserInfo?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<UserInfo?>(null);
        public Task<UserInfo?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default) => Task.FromResult<UserInfo?>(null);
        public Task<UserInfo?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult<UserInfo?>(null);
        public Task<UserInfo?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default) => Task.FromResult<UserInfo?>(null);
        public Task<UserInfo?> GetByDeviceUserIdGlobalAsync(string deviceUserId, CancellationToken cancellationToken = default) => Task.FromResult<UserInfo?>(null);
        public Task<UserInfo?> GetByDeviceUserIdAsync(string deviceUserId, string deviceIp, CancellationToken cancellationToken = default) => Task.FromResult<UserInfo?>(null);
        public Task<List<string>> GetAllUsernamesAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<string>());
        public Task<UserInfo> CreateAsync(UserInfo user, CancellationToken cancellationToken = default) => Task.FromResult(user);
        public Task UpdateAsync(UserInfo user, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<List<UserInfo>> GetAllUsersAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<UserInfo>());
        public Task<List<UserInfo>> GetUsersAsync(string? deviceIp, CancellationToken cancellationToken = default) => Task.FromResult(new List<UserInfo>());
        public Task<List<UserInfo>> SearchUsersByNameAsync(string searchTerm, CancellationToken cancellationToken = default) => Task.FromResult(new List<UserInfo>());
        public Task<List<UserInfo>> GetUsersByDepartmentAsync(Department department, CancellationToken cancellationToken = default) => Task.FromResult(new List<UserInfo>());
        public Task<List<UserInfo>> GetUsersByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken = default) => Task.FromResult(new List<UserInfo>());
        public Task<List<UserInfo>> GetActiveUsersAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<UserInfo>());
        public Task<UserInfo?> GetByIdWithHRsAsync(int userId) => Task.FromResult<UserInfo?>(null);
        public Task<List<UserInfo>> GetUsersWithEmailAsync(CancellationToken cancellationToken = default) => Task.FromResult(new List<UserInfo>());
        public Task<int> GetCountAsync(string? deviceIp = null, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<List<UserCountByDepartmentDto>> GetCountByDepartmentAsync(string? deviceIp = null, CancellationToken cancellationToken = default) => Task.FromResult(new List<UserCountByDepartmentDto>());
        public Task<List<Department>> GetDepartmentsByManagerIdAsync(int managerId, CancellationToken cancellationToken = default) => Task.FromResult(new List<Department>());
    }
}
