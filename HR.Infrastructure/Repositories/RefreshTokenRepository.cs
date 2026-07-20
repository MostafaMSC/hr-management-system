using System;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
using HR.Domain.Entities;

namespace HR.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default) => Task.FromResult<RefreshToken?>(null);
        public Task<RefreshToken> CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default) => Task.FromResult(refreshToken);
        public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RevokeAllUserTokensAsync(int userId, string reason, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
