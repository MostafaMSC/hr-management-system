using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HR.Domain.Entities;
using HR.Domain.Enums;
using HR.Application.Attendance.ZKPython.DTOs;

namespace HR.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        // ========================
        // Basic CRUD
        // ========================
        Task<UserInfo?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<UserInfo?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<UserInfo?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<UserInfo?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default);
        /// <summary>
        /// Gets a user by DeviceUserID regardless of the device IP.
        /// </summary>
        Task<UserInfo?> GetByDeviceUserIdGlobalAsync(string deviceUserId, CancellationToken cancellationToken = default);

        Task<UserInfo?> GetByDeviceUserIdAsync(string deviceUserId, string deviceIp, CancellationToken cancellationToken = default);
        Task<List<string>> GetAllUsernamesAsync(CancellationToken cancellationToken = default);
        Task<UserInfo> CreateAsync(UserInfo user, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserInfo user, CancellationToken cancellationToken = default);
        Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default);

        // ========================
        // Existence checks
        // ========================
        Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

        // ========================
        // Queries
        // ========================
        Task<List<UserInfo>> GetAllUsersAsync(CancellationToken cancellationToken = default);
        Task<List<UserInfo>> GetUsersAsync(string? deviceIp, CancellationToken cancellationToken = default);
        Task<List<UserInfo>> SearchUsersByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
        Task<List<UserInfo>> GetUsersByDepartmentAsync(Department department, CancellationToken cancellationToken = default);
        Task<List<UserInfo>> GetUsersByDepartmentIdAsync(int departmentId, CancellationToken cancellationToken = default);
        Task<List<UserInfo>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
        Task<UserInfo?> GetByIdWithHRsAsync(int userId);
        Task<List<UserInfo>> GetUsersWithEmailAsync(CancellationToken cancellationToken = default);


        
        // Count Queries
        Task<int> GetCountAsync(string? deviceIp = null, CancellationToken cancellationToken = default);
        Task<List<UserCountByDepartmentDto>> GetCountByDepartmentAsync(string? deviceIp = null, CancellationToken cancellationToken = default);
        
        // Department Manager Queries
        Task<List<Department>> GetDepartmentsByManagerIdAsync(int managerId, CancellationToken cancellationToken = default);
    }
}
