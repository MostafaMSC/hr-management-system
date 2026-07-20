using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HR.Domain.Entities;
using HR.Application.Common.Interfaces;
namespace HR.Infrastructure.Repositories
{
    public class LeaveRepository : ILeaveRepository
    {
        public Task AddAsync(LeaveRequest request) => Task.CompletedTask;
        public Task<LeaveRequest?> GetByIdAsync(int id) => Task.FromResult<LeaveRequest?>(null);
        public Task UpdateAsync(LeaveRequest request) => Task.CompletedTask;
        public Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId) => Task.FromResult<IEnumerable<LeaveRequest>>(new List<LeaveRequest>());
        public Task<IEnumerable<LeaveRequest>> GetPendingForManagerAsync(int departmentId) => Task.FromResult<IEnumerable<LeaveRequest>>(new List<LeaveRequest>());
        public Task<IEnumerable<LeaveRequest>> GetPendingForHRAsync() => Task.FromResult<IEnumerable<LeaveRequest>>(new List<LeaveRequest>());
        public Task<LeaveBalance?> GetLeaveBalanceAsync(int employeeId, int year) => Task.FromResult<LeaveBalance?>(null);
    }
}
