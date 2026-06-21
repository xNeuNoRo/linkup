using System.Data;

namespace LinkUpPro.Domain.Interfaces.Persistence;

/// <summary>
/// Coordinates atomic persistence operations without exposing infrastructure details to Application.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    bool HasActiveTransaction { get; }

    Task BeginTransactionAsync(
        CancellationToken cancellationToken = default,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
