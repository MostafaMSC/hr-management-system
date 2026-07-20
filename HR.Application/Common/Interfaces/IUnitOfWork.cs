namespace HR.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern to ensure transactional integrity across multiple repositories.
/// Wraps ApplicationDbContext.SaveChangesAsync() to allow multiple repository operations
/// to be committed as a single database transaction.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Commits all pending changes to the database as a single transaction.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
