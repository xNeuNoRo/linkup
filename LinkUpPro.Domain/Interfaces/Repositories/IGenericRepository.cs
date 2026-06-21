using System.Linq.Expressions;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Domain.Interfaces.Repositories;

/// <summary>
/// Generic repository contract for common asynchronous data access operations.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
/// <typeparam name="TId">Entity identifier type.</typeparam>
public interface IGenericRepository<T, TId>
    where T : BaseEntity<TId>
{
    Task<IReadOnlyCollection<T>> GetAllAsync(
        QueryOptions<T>? options = null,
        CancellationToken cancellationToken = default
    );

    Task<T?> GetFirstOrDefaultAsync(
        QueryOptions<T> options,
        CancellationToken cancellationToken = default
    );

    Task<T?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes
    );

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    void Update(T entity);

    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );
}
