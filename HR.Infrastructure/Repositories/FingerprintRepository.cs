using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;

namespace HR.Infrastructure.Repositories
{
    public class FingerprintRepository : IFingerprintRepository
    {
        private readonly IApplicationDbContext _context;
        public FingerprintRepository(IApplicationDbContext context) => _context = context;

        public Task<Fingerprint> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<Fingerprint>(null!);
        public Task<IEnumerable<Fingerprint>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Fingerprint>>(new List<Fingerprint>());
        public Task<IEnumerable<Fingerprint>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Fingerprint>>(new List<Fingerprint>());
        public Task<Fingerprint?> GetByUserIdAndFingerIndexAsync(int userId, int fingerIndex, CancellationToken cancellationToken = default) => Task.FromResult<Fingerprint?>(null);
        public Task<Fingerprint> AddAsync(Fingerprint fingerprint, CancellationToken cancellationToken = default) => Task.FromResult(fingerprint);
        public Task UpdateAsync(Fingerprint fingerprint, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Fingerprint fingerprint, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> ExistsAsync(int userId, int fingerIndex, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<List<Fingerprint>> AddRangeAsync(IEnumerable<Fingerprint> fingerprints, CancellationToken cancellationToken = default) => Task.FromResult(new List<Fingerprint>());
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
