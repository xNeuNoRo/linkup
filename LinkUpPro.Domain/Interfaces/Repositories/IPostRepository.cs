using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Domain.Interfaces.Repositories;

public interface IPostRepository : IGenericRepository<Post, long>
{
    Task<IReadOnlyCollection<Post>> GetByAuthorAsync(
        string authorId,
        QueryOptions<Post>? options = null,
        CancellationToken cancellationToken = default
    );

    Task<Post?> GetByIdWithDetailsAsync(long postId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Post>> GetVisibleFriendsPostsAsync(
        string userId,
        QueryOptions<Post>? options = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<Post>> SearchAuthorPostsAsync(
        string authorId,
        string? searchText,
        PostContentType? contentType,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        bool? editedOnly,
        QueryOptions<Post>? options = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<Post>> SearchFriendsPostsAsync(
        string userId,
        string? searchText,
        string? friendId,
        PostContentType? contentType,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        bool? editedOnly,
        QueryOptions<Post>? options = null,
        CancellationToken cancellationToken = default
    );

    Task<int> CountAvailableFriendsPostsAsync(
        string userId,
        CancellationToken cancellationToken = default
    );
}
