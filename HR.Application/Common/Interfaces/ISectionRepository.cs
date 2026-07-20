using HR.Domain.Entities;

namespace HR.Application.Common.Interfaces
{
    public interface ISectionRepository
    {
        Task<List<Section>> GetAllAsync(int? departmentId = null);
        Task<Section?> GetByIdAsync(int id);
        Task<Section?> GetByNameAsync(string name, int departmentId);
        Task<Section> AddAsync(Section section);
        Task<Section> UpdateAsync(Section section);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
