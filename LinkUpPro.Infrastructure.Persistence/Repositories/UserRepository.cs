using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Identity;
using LinkUpPro.Domain.Interfaces.Repositories;
using LinkUpPro.Domain.ValueObjects;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : GenericRepository<User, string>, IUserRepository
{
    public UserRepository(AppDbContext context)
        : base(context) { }

    public async Task<User?> GetByEmailAsync(
        Email email,
        CancellationToken cancellationToken = default
    )
    {
        var normalized = email.Value.ToUpperInvariant();
        return await _dbSet.FirstOrDefaultAsync(
            u => u.NormalizedEmail == normalized,
            cancellationToken
        );
    }

    public async Task<User?> GetByUserNameAsync(
        string userName,
        CancellationToken cancellationToken = default
    )
    {
        var normalized = userName.Trim().ToUpperInvariant();
        return await _dbSet.FirstOrDefaultAsync(
            u => u.NormalizedUserName == normalized,
            cancellationToken
        );
    }

    public async Task<bool> ExistsByEmailAsync(
        Email email,
        string? excludeUserId = null,
        CancellationToken cancellationToken = default
    )
    {
        var normalized = email.Value.ToUpperInvariant();

        return await _dbSet.AnyAsync(
            u => u.NormalizedEmail == normalized && u.Id != excludeUserId,
            cancellationToken
        );
    }

    public async Task<bool> ExistsByUserNameAsync(
        string userName,
        string? excludeUserId = null,
        CancellationToken cancellationToken = default
    )
    {
        var normalized = userName.Trim().ToUpperInvariant();

        return await _dbSet.AnyAsync(
            u => u.NormalizedUserName == normalized && u.Id != excludeUserId,
            cancellationToken
        );
    }

    public async Task<bool> IsActiveAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.AnyAsync(u => u.Id == userId && u.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> GetActiveUsersAsync(
        QueryOptions<User>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(u => u.IsActive);
        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> SearchActiveUsersAsync(
        string? searchTerm,
        QueryOptions<User>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(u => u.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();

            query = query.Where(u =>
                u.UserName.ToLower().Contains(term)
                || u.FirstName.ToLower().Contains(term)
                || u.LastName.ToLower().Contains(term)
                || u.Email.ToLower().Contains(term)
            );
        }

        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    private IQueryable<User> ApplyOptionsToQuery(
        IQueryable<User> query,
        QueryOptions<User>? options
    )
    {
        if (options is null)
            return query.AsNoTracking();

        if (!options.IsTracking)
            query = query.AsNoTracking();

        foreach (var include in options.Includes)
            query = query.Include(include);

        if (options.Filter is not null)
            query = query.Where(options.Filter);

        if (options.OrderBy is not null)
            query = options.OrderBy(query);

        if (options.Skip.HasValue)
            query = query.Skip(options.Skip.Value);

        if (options.Take.HasValue)
            query = query.Take(options.Take.Value);

        return query;
    }
}
