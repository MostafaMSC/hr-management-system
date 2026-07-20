using System.Collections.Generic;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;
namespace HR.Infrastructure.Repositories
{
    public class SectionRepository : ISectionRepository
    {
        public Task<List<Section>> GetAllAsync(int? departmentId = null) => Task.FromResult(new List<Section>());
        public Task<Section?> GetByIdAsync(int id) => Task.FromResult<Section?>(null);
        public Task<Section?> GetByNameAsync(string name, int departmentId) => Task.FromResult<Section?>(null);
        public Task<Section> AddAsync(Section section) => Task.FromResult(section);
        public Task<Section> UpdateAsync(Section section) => Task.FromResult(section);
        public Task<bool> DeleteAsync(int id) => Task.FromResult(true);
        public Task<bool> ExistsAsync(int id) => Task.FromResult(true);
    }
}
