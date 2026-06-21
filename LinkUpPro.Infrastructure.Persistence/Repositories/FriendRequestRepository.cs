using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Friendship;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.Interfaces.Repositories;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Repositories;

public sealed class FriendRequestRepository
    : GenericRepository<FriendRequest, long>,
        IFriendRequestRepository
{
    public FriendRequestRepository(AppDbContext context)
        : base(context) { }

    public async Task<IReadOnlyCollection<FriendRequest>> GetPendingReceivedAsync(
        string receiverId,
        QueryOptions<FriendRequest>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(r =>
            r.ReceiverId == receiverId && r.Status == FriendRequestStatus.Pending
        );
        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<FriendRequest>> GetPendingSentAsync(
        string senderId,
        QueryOptions<FriendRequest>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(r =>
            r.SenderId == senderId && r.Status == FriendRequestStatus.Pending
        );
        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<FriendRequest>> GetVisibleSentHistoryAsync(
        string senderId,
        QueryOptions<FriendRequest>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(r =>
            r.SenderId == senderId
            && r.IsVisibleForSender
            && r.Status != FriendRequestStatus.Pending
            && r.Status != FriendRequestStatus.Canceled
        );
        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsPendingBetweenAsync(
        string firstUserId,
        string secondUserId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.AnyAsync(
            r =>
                (
                    (r.SenderId == firstUserId && r.ReceiverId == secondUserId)
                    || (r.SenderId == secondUserId && r.ReceiverId == firstUserId)
                )
                && r.Status == FriendRequestStatus.Pending,
            cancellationToken
        );
    }

    public async Task<FriendRequest?> GetPendingBetweenAsync(
        string firstUserId,
        string secondUserId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.FirstOrDefaultAsync(
            r =>
                (
                    (r.SenderId == firstUserId && r.ReceiverId == secondUserId)
                    || (r.SenderId == secondUserId && r.ReceiverId == firstUserId)
                )
                && r.Status == FriendRequestStatus.Pending,
            cancellationToken
        );
    }

    public async Task<FriendRequest?> GetByIdForSenderAsync(
        long requestId,
        string senderId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.FirstOrDefaultAsync(
            r => r.Id == requestId && r.SenderId == senderId,
            cancellationToken
        );
    }

    public async Task<FriendRequest?> GetByIdForReceiverAsync(
        long requestId,
        string receiverId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.FirstOrDefaultAsync(
            r => r.Id == requestId && r.ReceiverId == receiverId,
            cancellationToken
        );
    }

    private static IQueryable<FriendRequest> ApplyOptionsToQuery(
        IQueryable<FriendRequest> query,
        QueryOptions<FriendRequest>? options
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
