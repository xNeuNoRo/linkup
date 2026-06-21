using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Interfaces.Repositories;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository
    : GenericRepository<Notification, long>,
        INotificationRepository
{
    public NotificationRepository(AppDbContext context)
        : base(context) { }

    public async Task<IReadOnlyCollection<Notification>> GetByRecipientAsync(
        string recipientId,
        QueryOptions<Notification>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(n => n.RecipientId == recipientId);
        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Notification>> GetRecentAsync(
        string recipientId,
        int count,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet
            .Where(n => n.RecipientId == recipientId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(
        string recipientId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.CountAsync(
            n => n.RecipientId == recipientId && !n.IsRead,
            cancellationToken
        );
    }

    public async Task<Notification?> GetForRecipientAsync(
        long notificationId,
        string recipientId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.FirstOrDefaultAsync(
            n => n.Id == notificationId && n.RecipientId == recipientId,
            cancellationToken
        );
    }

    public async Task MarkAsReadAsync(
        long notificationId,
        string recipientId,
        CancellationToken cancellationToken = default
    )
    {
        var notification = await _dbSet.FirstOrDefaultAsync(
            n => n.Id == notificationId && n.RecipientId == recipientId,
            cancellationToken
        );

        if (notification is not null)
        {
            notification.MarkAsRead();
        }
    }

    public async Task MarkAllAsReadAsync(
        string recipientId,
        CancellationToken cancellationToken = default
    )
    {
        var unread = await _dbSet
            .Where(n => n.RecipientId == recipientId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unread)
        {
            notification.MarkAsRead();
        }
    }

    private static IQueryable<Notification> ApplyOptionsToQuery(
        IQueryable<Notification> query,
        QueryOptions<Notification>? options
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
