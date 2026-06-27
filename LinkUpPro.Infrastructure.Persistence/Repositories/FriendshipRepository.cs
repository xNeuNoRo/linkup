using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Enums;
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

    public async Task<Dictionary<string, int>> GetCommonFriendsCountForUsersAsync(
        string userId,
        IReadOnlyCollection<string> targetUserIds,
        CancellationToken cancellationToken = default)
    {
        var targetList = targetUserIds.ToList();
        if (targetList.Count == 0)
            return new Dictionary<string, int>();

        // Query 1: Get userId's friends
        var userFriendIds = await _dbSet
            .Where(f => f.User1Id == userId || f.User2Id == userId)
            .Select(f => f.User1Id == userId ? f.User2Id : f.User1Id)
            .ToListAsync(cancellationToken);
        var userFriendIdsSet = userFriendIds.ToHashSet();

        // Query 2: Get friendships for ALL target users in one query (batch)
        var friendships = await _dbSet
            .Where(f => targetList.Contains(f.User1Id) || targetList.Contains(f.User2Id))
            .Select(f => new { f.User1Id, f.User2Id })
            .ToListAsync(cancellationToken);

        // Process in memory: count common friends per target user
        var result = new Dictionary<string, int>(targetList.Count);
        foreach (var targetId in targetList)
        {
            var targetFriends = friendships
                .Where(f => f.User1Id == targetId || f.User2Id == targetId)
                .Select(f => f.User1Id == targetId ? f.User2Id : f.User1Id);
            result[targetId] = targetFriends.Intersect(userFriendIdsSet).Count();
        }

        return result;
    }

    public async Task<IReadOnlyCollection<string>> GetFriendIdsPagedAsync(
        string userId,
        QueryOptions<DomainFriendship> options,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet
            .Where(f => f.User1Id == userId || f.User2Id == userId)
            .Select(f => f.User1Id == userId ? f.User2Id : f.User1Id);

        return await ApplyOptionsToQueryString(query, options).ToListAsync(cancellationToken);
    }

    public async Task<int> GetActiveFriendsCountWithSearchAsync(
        string userId,
        IReadOnlyCollection<string> filteredFriendIds,
        CancellationToken cancellationToken = default
    )
    {
        var friendSet = await _dbSet
            .Where(f => f.User1Id == userId || f.User2Id == userId)
            .Select(f => f.User1Id == userId ? f.User2Id : f.User1Id)
            .ToListAsync(cancellationToken);

        return friendSet.Count(id => filteredFriendIds.Contains(id));
    }

    public async Task<IReadOnlyCollection<string>> GetCommonFriendIdsPagedAsync(
        string firstUserId,
        string secondUserId,
        QueryOptions<DomainFriendship> options,
        CancellationToken cancellationToken = default
    )
    {
        var firstFriendIds = await _dbSet
            .Where(f => f.User1Id == firstUserId || f.User2Id == firstUserId)
            .Select(f => f.User1Id == firstUserId ? f.User2Id : f.User1Id)
            .ToListAsync(cancellationToken);
        var firstSet = firstFriendIds.ToHashSet();

        var secondQuery = _dbSet
            .Where(f => f.User1Id == secondUserId || f.User2Id == secondUserId)
            .Where(f => firstSet.Contains(f.User1Id == secondUserId ? f.User2Id : f.User1Id))
            .Select(f => f.User1Id == secondUserId ? f.User2Id : f.User1Id);

        return await ApplyOptionsToQueryString(secondQuery, options).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<string>> GetPendingRequestUserIdsAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Set<Domain.Entities.Friendship.FriendRequest>()
            .Where(r => (r.SenderId == userId || r.ReceiverId == userId)
                && r.Status == FriendRequestStatus.Pending)
            .Select(r => r.SenderId == userId ? r.ReceiverId : r.SenderId)
            .ToListAsync(cancellationToken);
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

    private static IQueryable<string> ApplyOptionsToQueryString(
        IQueryable<string> query,
        QueryOptions<DomainFriendship>? options
    )
    {
        if (options is null)
            return query;

        if (options.Filter is not null)
            return query; // Filter can't be applied after Select projection easily

        if (options.OrderBy is not null)
            return query; // OrderBy can't be applied after Select projection easily

        if (options.Skip.HasValue)
            query = query.Skip(options.Skip.Value);

        if (options.Take.HasValue)
            query = query.Take(options.Take.Value);

        return query;
    }
}
