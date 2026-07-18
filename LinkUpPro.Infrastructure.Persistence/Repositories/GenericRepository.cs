using System.Linq.Expressions;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Interfaces.Repositories;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Repositories;

public class GenericRepository<T, TId> : IGenericRepository<T, TId>
    where T : BaseEntity<TId>
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<IReadOnlyCollection<T>> GetAllAsync(
        QueryOptions<T>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        return await ApplyOptions(options).ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> GetFirstOrDefaultAsync(
        QueryOptions<T> options,
        CancellationToken cancellationToken = default
    )
    {
        return await ApplyOptions(options).FirstOrDefaultAsync(cancellationToken);
    }

    public virtual async Task<T?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes
    )
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(x => x.Id!.Equals(id), cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    )
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        return predicate is null
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(predicate, cancellationToken);
    }

    protected IQueryable<T> ApplyOptions(QueryOptions<T>? options)
    {
        IQueryable<T> query = _dbSet;

        if (options is null)
        {
            return query.AsNoTracking();
        }

        // Control de Tracking
        if (!options.IsTracking)
        {
            query = query.AsNoTracking();
        }

        // Inclusiones (Include / ThenInclude)
        foreach (var include in options.Includes)
        {
            query = query.Include(include);
        }

        // Filtrado (Where)
        if (options.Filter is not null)
        {
            query = query.Where(options.Filter);
        }

        // Ordenamiento (OrderBy)
        if (options.OrderBy is not null)
        {
            query = options.OrderBy(query);
        }

        // Paginación (Skip / Take)
        if (options.Skip.HasValue)
        {
            query = query.Skip(options.Skip.Value);
        }

        if (options.Take.HasValue)
        {
            query = query.Take(options.Take.Value);
        }

        return query;
    }
}
