using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Interfaces.Repositories;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Repositories;

public sealed class CommentRepository : GenericRepository<Comment, long>, ICommentRepository
{
    public CommentRepository(AppDbContext context)
        : base(context) { }

    public async Task<IReadOnlyCollection<Comment>> GetByPostAsync(
        long postId,
        QueryOptions<Comment>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(c => c.PostId == postId);
        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Comment>> GetRepliesAsync(
        long parentCommentId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Where(c => c.ParentCommentId == parentCommentId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Comment>> GetThreadAsync(
        long rootCommentId,
        CancellationToken cancellationToken = default
    )
    {
        var root = await _dbSet.FirstOrDefaultAsync(c => c.Id == rootCommentId, cancellationToken);

        if (root is null)
            return Array.Empty<Comment>();

        var allPostComments = await _dbSet
            .Where(c => c.PostId == root.PostId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var threadIds = CollectDescendantIds(rootCommentId, allPostComments);
        threadIds.Add(rootCommentId);

        return allPostComments.Where(c => threadIds.Contains(c.Id)).ToList();
    }

    public async Task<IReadOnlyCollection<Comment>> GetByAuthorAsync(
        string authorId,
        QueryOptions<Comment>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(c => c.AuthorId == authorId);
        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<int> CountByPostAsync(
        long postId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.CountAsync(c => c.PostId == postId, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<long, int>> GetCountsForPostsAsync(
        IEnumerable<long> postIds,
        CancellationToken cancellationToken = default
    )
    {
        var ids = postIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<long, int>();

        return await _dbSet
            .Where(c => ids.Contains(c.PostId))
            .GroupBy(c => c.PostId)
            .Select(g => new { PostId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PostId, x => x.Count, cancellationToken);
    }

    public async Task<bool> HasRepliesAsync(
        long commentId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.AnyAsync(c => c.ParentCommentId == commentId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Comment>> GetRootCommentsByPostAsync(
        long postId,
        QueryOptions<Comment> options,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(c => c.PostId == postId && c.ParentCommentId == null);
        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Comment>> GetRepliesByParentAsync(
        long parentCommentId,
        QueryOptions<Comment> options,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(c => c.ParentCommentId == parentCommentId);
        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<int> CountRootCommentsByPostAsync(
        long postId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.CountAsync(
            c => c.PostId == postId && c.ParentCommentId == null,
            cancellationToken
        );
    }

    public async Task<int> CountRepliesByParentAsync(
        long parentCommentId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.CountAsync(
            c => c.ParentCommentId == parentCommentId,
            cancellationToken
        );
    }

    private static HashSet<long> CollectDescendantIds(
        long parentId,
        IReadOnlyCollection<Comment> allComments
    )
    {
        var ids = new HashSet<long>();
        var directReplies = allComments.Where(c => c.ParentCommentId == parentId).ToList();

        foreach (var reply in directReplies)
        {
            ids.Add(reply.Id);
            ids.UnionWith(CollectDescendantIds(reply.Id, allComments));
        }

        return ids;
    }

    private static IQueryable<Comment> ApplyOptionsToQuery(
        IQueryable<Comment> query,
        QueryOptions<Comment>? options
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
