using System.Data;

namespace LinkUpPro.Domain.Interfaces.Persistence;

/// <summary>
/// Interfaz que define el contrato para la unidad de trabajo, que maneja las transacciones
/// y la persistencia de datos en el contexto del dominio.
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    bool HasActiveTransaction { get; }

    Task BeginTransactionAsync(
        CancellationToken cancellationToken = default,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted
    );

    Task CommitAsync(CancellationToken cancellationToken = default);

    Task RollbackAsync(CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
