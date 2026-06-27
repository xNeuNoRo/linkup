using LinkUpPro.Domain.Common;
using DomainFriendship = LinkUpPro.Domain.Entities.Friendship.Friendship;

namespace LinkUpPro.Domain.Interfaces.Repositories;

public interface IFriendshipRepository : IGenericRepository<DomainFriendship, long>
{
    Task<bool> AreFriendsAsync(
        string firstUserId,
        string secondUserId,
        CancellationToken cancellationToken = default
    );

    Task<DomainFriendship?> GetFriendshipBetweenAsync(
        string firstUserId,
        string secondUserId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<DomainFriendship>> GetActiveFriendshipsForUserAsync(
        string userId,
        QueryOptions<DomainFriendship>? options = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<string>> GetActiveFriendIdsAsync(
        string userId,
        CancellationToken cancellationToken = default
    );

    Task<int> GetActiveFriendsCountAsync(
        string userId,
        CancellationToken cancellationToken = default
    );

    Task<int> GetCommonFriendsCountAsync(
        string firstUserId,
        string secondUserId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<string>> GetCommonFriendIdsAsync(
        string firstUserId,
        string secondUserId,
        CancellationToken cancellationToken = default
    );

    Task<Dictionary<string, int>> GetCommonFriendsCountForUsersAsync(
        string userId,
        IReadOnlyCollection<string> targetUserIds,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<string>> GetFriendIdsPagedAsync(
        string userId,
        QueryOptions<DomainFriendship> options,
        CancellationToken cancellationToken = default
    );

    Task<int> GetActiveFriendsCountWithSearchAsync(
        string userId,
        IReadOnlyCollection<string> filteredFriendIds,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<string>> GetCommonFriendIdsPagedAsync(
        string firstUserId,
        string secondUserId,
        QueryOptions<DomainFriendship> options,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<string>> GetPendingRequestUserIdsAsync(
        string userId,
        CancellationToken cancellationToken = default
    );
}
