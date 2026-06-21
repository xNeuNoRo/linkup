using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Friendship;

namespace LinkUpPro.Domain.Interfaces.Repositories;

public interface IFriendRequestRepository : IGenericRepository<FriendRequest, long>
{
    Task<IReadOnlyCollection<FriendRequest>> GetPendingReceivedAsync(
        string receiverId,
        QueryOptions<FriendRequest>? options = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<FriendRequest>> GetPendingSentAsync(
        string senderId,
        QueryOptions<FriendRequest>? options = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<FriendRequest>> GetVisibleSentHistoryAsync(
        string senderId,
        QueryOptions<FriendRequest>? options = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsPendingBetweenAsync(
        string firstUserId,
        string secondUserId,
        CancellationToken cancellationToken = default
    );

    Task<FriendRequest?> GetPendingBetweenAsync(
        string firstUserId,
        string secondUserId,
        CancellationToken cancellationToken = default
    );

    Task<FriendRequest?> GetByIdForSenderAsync(
        long requestId,
        string senderId,
        CancellationToken cancellationToken = default
    );

    Task<FriendRequest?> GetByIdForReceiverAsync(
        long requestId,
        string receiverId,
        CancellationToken cancellationToken = default
    );
}
