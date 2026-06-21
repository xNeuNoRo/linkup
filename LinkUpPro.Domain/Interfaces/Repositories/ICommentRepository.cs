using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;

namespace LinkUpPro.Domain.Interfaces.Repositories;

public interface ICommentRepository : IGenericRepository<Comment, long>
{
    Task<IReadOnlyCollection<Comment>> GetByPostAsync(
        long postId,
        QueryOptions<Comment>? options = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Comment>> GetRepliesAsync(
        long parentCommentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Comment>> GetThreadAsync(
        long rootCommentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Comment>> GetByAuthorAsync(
        string authorId,
        QueryOptions<Comment>? options = null,
        CancellationToken cancellationToken = default);

    Task<int> CountByPostAsync(long postId, CancellationToken cancellationToken = default);

    Task<bool> HasRepliesAsync(long commentId, CancellationToken cancellationToken = default);
}
