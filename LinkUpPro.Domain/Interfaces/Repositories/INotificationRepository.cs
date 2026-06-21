using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;

namespace LinkUpPro.Domain.Interfaces.Repositories;

public interface INotificationRepository : IGenericRepository<Notification, long>
{
    Task<IReadOnlyCollection<Notification>> GetByRecipientAsync(
        string recipientId,
        QueryOptions<Notification>? options = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<Notification>> GetRecentAsync(
        string recipientId,
        int count,
        CancellationToken cancellationToken = default
    );

    Task<int> GetUnreadCountAsync(
        string recipientId,
        CancellationToken cancellationToken = default
    );

    Task<Notification?> GetForRecipientAsync(
        long notificationId,
        string recipientId,
        CancellationToken cancellationToken = default
    );

    Task MarkAsReadAsync(
        long notificationId,
        string recipientId,
        CancellationToken cancellationToken = default
    );

    Task MarkAllAsReadAsync(string recipientId, CancellationToken cancellationToken = default);
}
