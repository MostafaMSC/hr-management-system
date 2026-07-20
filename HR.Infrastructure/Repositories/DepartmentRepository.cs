using System.Collections.Generic;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
namespace HR.Infrastructure.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        public Task<List<Department>> GetAllAsync() => Task.FromResult(new List<Department>());
        public Task<Department?> GetByIdAsync(int id) => Task.FromResult<Department?>(null);
        public Task<Department?> GetByNameAsync(string name) => Task.FromResult<Department?>(null);
        public Task<Department> AddAsync(Department department) => Task.FromResult(department);
        public Task<Department> UpdateAsync(Department department) => Task.FromResult(department);
        public Task<bool> DeleteAsync(int id) => Task.FromResult(true);
        public Task<bool> ExistsAsync(int id) => Task.FromResult(true);
        public Task<int> GetSectionCountAsync(int departmentId) => Task.FromResult(0);
        public Task<int> GetUserCountAsync(int departmentId) => Task.FromResult(0);
    }
}
