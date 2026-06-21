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
}
