namespace HR.Application.Common.Interfaces;

/// <summary>
/// Abstraction over distributed caching (Redis).
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get a cached value by key. Returns default(T) if not found.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set a value in cache with an optional expiration.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a specific key from cache.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove all keys matching a prefix pattern.
    /// </summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
