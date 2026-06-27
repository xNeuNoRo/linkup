using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Friendship;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.Interfaces.Repositories;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Repositories;

public sealed class PostRepository : GenericRepository<Post, long>, IPostRepository
{
    public PostRepository(AppDbContext context)
        : base(context) { }

    public async Task<IReadOnlyCollection<Post>> GetByAuthorAsync(
        string authorId,
        QueryOptions<Post>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(p => p.AuthorId == authorId);
        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<Post?> GetByIdWithDetailsAsync(
        long postId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Post>> GetVisibleFriendsPostsAsync(
        string userId,
        QueryOptions<Post>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var friendIds = await _context
            .Set<Friendship>()
            .Where(f => f.User1Id == userId || f.User2Id == userId)
            .Select(f => f.User1Id == userId ? f.User2Id : f.User1Id)
            .ToListAsync(cancellationToken);

        var query = _dbSet.Where(p =>
            friendIds.Contains(p.AuthorId) && p.Privacy == PrivacyLevel.FriendsOnly
        );

        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Post>> SearchAuthorPostsAsync(
        string authorId,
        string? searchText,
        PostContentType? contentType,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        bool? editedOnly,
        QueryOptions<Post>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(p => p.AuthorId == authorId);

        query = ApplySearchFilters(query, searchText, contentType, fromDate, toDate, editedOnly);

        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Post>> SearchFriendsPostsAsync(
        string userId,
        string? searchText,
        string? friendId,
        PostContentType? contentType,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        bool? editedOnly,
        QueryOptions<Post>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var friendIdsQuery = _context
            .Set<Friendship>()
            .Where(f => f.User1Id == userId || f.User2Id == userId);

        IQueryable<string> friendIdResults;

        if (!string.IsNullOrWhiteSpace(friendId))
        {
            friendIdResults = friendIdsQuery
                .Where(f =>
                    (f.User1Id == userId && f.User2Id == friendId)
                    || (f.User2Id == userId && f.User1Id == friendId)
                )
                .Select(f => f.User1Id == userId ? f.User2Id : f.User1Id);
        }
        else
        {
            friendIdResults = friendIdsQuery.Select(f =>
                f.User1Id == userId ? f.User2Id : f.User1Id
            );
        }

        var ids = await friendIdResults.ToListAsync(cancellationToken);

        var query = _dbSet.Where(p =>
            ids.Contains(p.AuthorId) && p.Privacy == PrivacyLevel.FriendsOnly
        );

        query = ApplySearchFilters(query, searchText, contentType, fromDate, toDate, editedOnly);

        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAvailableFriendsPostsAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        var friendIds = await _context
            .Set<Domain.Entities.Friendship.Friendship>()
            .Where(f => f.User1Id == userId || f.User2Id == userId)
            .Select(f => f.User1Id == userId ? f.User2Id : f.User1Id)
            .ToListAsync(cancellationToken);

        if (friendIds.Count == 0)
            return 0;

        return await _dbSet.CountAsync(
            p => friendIds.Contains(p.AuthorId) && p.Privacy == PrivacyLevel.FriendsOnly,
            cancellationToken
        );
    }

    private static IQueryable<Post> ApplySearchFilters(
        IQueryable<Post> query,
        string? searchText,
        PostContentType? contentType,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        bool? editedOnly
    )
    {
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim().ToLower();
            query = query.Where(p => p.Content.ToLower().Contains(term));
        }

        if (contentType.HasValue)
        {
            query = query.Where(p => p.ContentType == contentType.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(p => p.CreatedAt <= endOfDay);
        }

        if (editedOnly.HasValue)
        {
            query = query.Where(p => p.IsEdited == editedOnly.Value);
        }

        return query;
    }

    private static IQueryable<Post> ApplyOptionsToQuery(
        IQueryable<Post> query,
        QueryOptions<Post>? options
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
