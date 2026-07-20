using System;
using System.Threading;
using System.Threading.Tasks;
using HR.Application.Common.Interfaces;
namespace HR.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) => Task.FromResult<T?>(default);
        public Task SetAsync<T>(string key, T value, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
