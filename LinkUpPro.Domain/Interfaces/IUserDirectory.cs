using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Friendship;

namespace LinkUpPro.Domain.Interfaces;

public interface IUserDirectory
{
    Task<IReadOnlyCollection<UserSearchResult>> SearchAvailableUsersPagedAsync(
        string currentUserId,
        string? searchTerm,
        QueryOptions<UserSearchResult> options,
        CancellationToken cancellationToken = default
    );

    Task<int> CountAvailableUsersAsync(
        string currentUserId,
        string? searchTerm,
        CancellationToken cancellationToken = default
    );
}
