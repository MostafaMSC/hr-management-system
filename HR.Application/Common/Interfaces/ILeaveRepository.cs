using HR.Domain.Entities;

namespace HR.Application.Common.Interfaces
{
    public interface ILeaveRepository
    {
        Task AddAsync(LeaveRequest request);
        Task<LeaveRequest?> GetByIdAsync(int id);
        Task UpdateAsync(LeaveRequest request);
        Task<IEnumerable<LeaveRequest>> GetByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<LeaveRequest>> GetPendingForManagerAsync(int departmentId);
        Task<IEnumerable<LeaveRequest>> GetPendingForHRAsync();

        // Leave balance â€“ new
        Task<LeaveBalance?> GetLeaveBalanceAsync(int employeeId, int year);
    }
}
