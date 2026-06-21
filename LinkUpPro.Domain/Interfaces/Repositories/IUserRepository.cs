using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Identity;
using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Domain.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User, string>
{
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(
        Email email,
        string? excludeUserId = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistsByUserNameAsync(
        string userName,
        string? excludeUserId = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> IsActiveAsync(string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<User>> GetActiveUsersAsync(
        QueryOptions<User>? options = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<User>> SearchActiveUsersAsync(
        string? searchTerm,
        QueryOptions<User>? options = null,
        CancellationToken cancellationToken = default
    );
}
