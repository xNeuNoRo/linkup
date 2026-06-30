using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.Interfaces.Repositories;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Repositories;

public sealed class ReactionRepository : GenericRepository<Reaction, long>, IReactionRepository
{
    public ReactionRepository(AppDbContext context)
        : base(context) { }

    public async Task<Reaction?> GetByPostAndUserAsync(
        long postId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.FirstOrDefaultAsync(
            r => r.PostId == postId && r.UserId == userId,
            cancellationToken
        );
    }

    public async Task<bool> ExistsByPostAndUserAsync(
        long postId,
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.AnyAsync(
            r => r.PostId == postId && r.UserId == userId,
            cancellationToken
        );
    }

    public async Task<ReactionCounts> GetCountsByPostAsync(
        long postId,
        CancellationToken cancellationToken = default
    )
    {
        var likes = await _dbSet.CountAsync(
            r => r.PostId == postId && r.Type == ReactionType.Like,
            cancellationToken
        );

        var dislikes = await _dbSet.CountAsync(
            r => r.PostId == postId && r.Type == ReactionType.Dislike,
            cancellationToken
        );

        return new ReactionCounts(likes, dislikes);
    }

    public async Task<IReadOnlyDictionary<long, ReactionCounts>> GetCountsForPostsAsync(
        IEnumerable<long> postIds,
        CancellationToken cancellationToken = default
    )
    {
        var ids = postIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<long, ReactionCounts>();

        var counts = await _dbSet
            .Where(r => ids.Contains(r.PostId))
            .GroupBy(r => r.PostId)
            .Select(g => new
            {
                PostId = g.Key,
                Likes = g.Count(r => r.Type == ReactionType.Like),
                Dislikes = g.Count(r => r.Type == ReactionType.Dislike)
            })
            .ToDictionaryAsync(x => x.PostId, x => new ReactionCounts(x.Likes, x.Dislikes), cancellationToken);

        return counts;
    }

    public async Task<int> CountByPostAndTypeAsync(
        long postId,
        ReactionType reactionType,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.CountAsync(
            r => r.PostId == postId && r.Type == reactionType,
            cancellationToken
        );
    }

    public async Task<IReadOnlyDictionary<long, Reaction?>> GetByUserAndPostsAsync(
        string userId,
        IEnumerable<long> postIds,
        CancellationToken cancellationToken = default
    )
    {
        var ids = postIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<long, Reaction?>();

        var reactions = await _dbSet
            .Where(r => r.UserId == userId && ids.Contains(r.PostId))
            .ToListAsync(cancellationToken);

        return ids.ToDictionary(id => id, id => reactions.FirstOrDefault(r => r.PostId == id));
    }

    public async Task<IReadOnlyCollection<Reaction>> GetByUserAsync(
        string userId,
        QueryOptions<Reaction>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(r => r.UserId == userId);
        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    private static IQueryable<Reaction> ApplyOptionsToQuery(
        IQueryable<Reaction> query,
        QueryOptions<Reaction>? options
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
