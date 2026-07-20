using HR.Domain.Entities;

namespace HR.Application.Common.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<List<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(int id);
        Task<Department?> GetByNameAsync(string name);
        Task<Department> AddAsync(Department department);
        Task<Department> UpdateAsync(Department department);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> GetSectionCountAsync(int departmentId);
        Task<int> GetUserCountAsync(int departmentId);
    }
}
