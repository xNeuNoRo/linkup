using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Interfaces.Repositories;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using DomainFriendship = LinkUpPro.Domain.Entities.Friendship.Friendship;

namespace LinkUpPro.Infrastructure.Persistence.Repositories;

public sealed class FriendshipRepository
    : GenericRepository<DomainFriendship, long>,
        IFriendshipRepository
{
    public FriendshipRepository(AppDbContext context)
        : base(context) { }

    public async Task<bool> AreFriendsAsync(
        string firstUserId,
        string secondUserId,
        CancellationToken cancellationToken = default
    )
    {
        var (user1Id, user2Id) = OrderIds(firstUserId, secondUserId);

        return await _dbSet.AnyAsync(
            f => f.User1Id == user1Id && f.User2Id == user2Id,
            cancellationToken
        );
    }

    public async Task<DomainFriendship?> GetFriendshipBetweenAsync(
        string firstUserId,
        string secondUserId,
        CancellationToken cancellationToken = default
    )
    {
        var (user1Id, user2Id) = OrderIds(firstUserId, secondUserId);

        return await _dbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                f => f.User1Id == user1Id && f.User2Id == user2Id,
                cancellationToken
            );
    }

    public async Task<IReadOnlyCollection<DomainFriendship>> GetActiveFriendshipsForUserAsync(
        string userId,
        QueryOptions<DomainFriendship>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(f => f.User1Id == userId || f.User2Id == userId);
        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetActiveFriendIdsAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Where(f => f.User1Id == userId || f.User2Id == userId)
            .Select(f => f.User1Id == userId ? f.User2Id : f.User1Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetActiveFriendsCountAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.CountAsync(
            f => f.User1Id == userId || f.User2Id == userId,
            cancellationToken
        );
    }

    public async Task<int> GetCommonFriendsCountAsync(
        string firstUserId,
        string secondUserId,
        CancellationToken cancellationToken = default
    )
    {
        var firstFriendIds = await GetActiveFriendIdsAsync(firstUserId, cancellationToken);
        var secondFriendIds = await GetActiveFriendIdsAsync(secondUserId, cancellationToken);

        return firstFriendIds.Intersect(secondFriendIds).Count();
    }

    public async Task<IReadOnlyCollection<string>> GetCommonFriendIdsAsync(
        string firstUserId,
        string secondUserId,
        CancellationToken cancellationToken = default
    )
    {
        var firstFriendIds = await GetActiveFriendIdsAsync(firstUserId, cancellationToken);
        var secondFriendIds = await GetActiveFriendIdsAsync(secondUserId, cancellationToken);

        return firstFriendIds.Intersect(secondFriendIds).ToList();
    }

    private static (string User1Id, string User2Id) OrderIds(
        string firstUserId,
        string secondUserId
    ) =>
        string.CompareOrdinal(firstUserId, secondUserId) <= 0
            ? (firstUserId, secondUserId)
            : (secondUserId, firstUserId);

    private static IQueryable<DomainFriendship> ApplyOptionsToQuery(
        IQueryable<DomainFriendship> query,
        QueryOptions<DomainFriendship>? options
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
