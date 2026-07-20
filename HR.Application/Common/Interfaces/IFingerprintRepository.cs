using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HR.Domain.Entities;

namespace HR.Application.Common.Interfaces
{
    public interface IFingerprintRepository
    {
        Task<Fingerprint> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Fingerprint>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Fingerprint>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<Fingerprint?> GetByUserIdAndFingerIndexAsync(int userId, int fingerIndex, CancellationToken cancellationToken = default);
        
        Task<Fingerprint> AddAsync(Fingerprint fingerprint, CancellationToken cancellationToken = default);
        Task UpdateAsync(Fingerprint fingerprint, CancellationToken cancellationToken = default);
        Task DeleteAsync(Fingerprint fingerprint, CancellationToken cancellationToken = default);
        
        Task<bool> ExistsAsync(int userId, int fingerIndex, CancellationToken cancellationToken = default);
        Task<List<Fingerprint>> AddRangeAsync(IEnumerable<Fingerprint> fingerprints, CancellationToken cancellationToken = default);
        
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
