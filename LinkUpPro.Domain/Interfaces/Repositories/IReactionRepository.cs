using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Domain.Interfaces.Repositories;

public interface IReactionRepository : IGenericRepository<Reaction, long>
{
    Task<Reaction?> GetByPostAndUserAsync(
        long postId,
        string userId,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByPostAndUserAsync(
        long postId,
        string userId,
        CancellationToken cancellationToken = default
    );

    Task<ReactionCounts> GetCountsByPostAsync(
        long postId,
        CancellationToken cancellationToken = default
    );

    Task<int> CountByPostAndTypeAsync(
        long postId,
        ReactionType reactionType,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<Reaction>> GetByUserAsync(
        string userId,
        QueryOptions<Reaction>? options = null,
        CancellationToken cancellationToken = default
    );
}
