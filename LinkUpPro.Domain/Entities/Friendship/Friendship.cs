using LinkUpPro.Domain.Common;

namespace LinkUpPro.Domain.Entities.Friendship;

public sealed class Friendship : AuditableBaseEntity<long>
{
    private Friendship() { }

    public string User1Id { get; private set; } = null!;

    public string User2Id { get; private set; } = null!;

    public static Result<Friendship> Create(
        string firstUserId,
        string secondUserId,
        DateTimeOffset? createdAt = null
    )
    {
        var errors = ValidateUsers(firstUserId, secondUserId);

        if (errors.Count > 0)
        {
            return Result<Friendship>.Failure(errors);
        }

        var (user1Id, user2Id) = OrderUserIds(firstUserId.Trim(), secondUserId.Trim());

        return Result<Friendship>.Success(
            new Friendship
            {
                User1Id = user1Id,
                User2Id = user2Id,
                CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
            }
        );
    }

    public override void MarkAsDeleted(DateTimeOffset? deletedAt = null) =>
        base.MarkAsDeleted(deletedAt);

    public void Reactivate(DateTimeOffset? updatedAt = null)
    {
        if (!IsDeleted)
        {
            return;
        }

        DeletedAt = null;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
    }

    public bool InvolvesUser(string userId) => User1Id == userId || User2Id == userId;

    public bool IsBetween(string firstUserId, string secondUserId)
    {
        if (string.IsNullOrWhiteSpace(firstUserId) || string.IsNullOrWhiteSpace(secondUserId))
        {
            return false;
        }

        var (user1Id, user2Id) = OrderUserIds(firstUserId.Trim(), secondUserId.Trim());
        return User1Id == user1Id && User2Id == user2Id;
    }

    public Result<string> GetFriendId(string currentUserId)
    {
        if (!InvolvesUser(currentUserId))
        {
            return Result<string>.Failure(
                new DomainError(
                    "Friendship.UserNotInFriendship",
                    "El usuario no forma parte de esta amistad."
                )
            );
        }

        return Result<string>.Success(User1Id == currentUserId ? User2Id : User1Id);
    }

    private static List<DomainError> ValidateUsers(string firstUserId, string secondUserId)
    {
        var errors = new List<DomainError>();

        if (string.IsNullOrWhiteSpace(firstUserId))
        {
            errors.Add(
                new DomainError("Friendship.FirstUserRequired", "El primer usuario es requerido.")
            );
        }

        if (string.IsNullOrWhiteSpace(secondUserId))
        {
            errors.Add(
                new DomainError("Friendship.SecondUserRequired", "El segundo usuario es requerido.")
            );
        }

        if (!string.IsNullOrWhiteSpace(firstUserId) && firstUserId.Trim() == secondUserId?.Trim())
        {
            errors.Add(
                new DomainError(
                    "Friendship.SelfFriendshipNotAllowed",
                    "Un usuario no puede agregarse a si mismo."
                )
            );
        }

        return errors;
    }

    private static (string User1Id, string User2Id) OrderUserIds(
        string firstUserId,
        string secondUserId
    ) =>
        string.CompareOrdinal(firstUserId, secondUserId) <= 0
            ? (firstUserId, secondUserId)
            : (secondUserId, firstUserId);
}
