using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Domain.Entities.Social;

public sealed class Reaction : BaseEntity<long>
{
    private Reaction()
    {
    }

    public long PostId { get; private set; }

    public string UserId { get; private set; } = null!;

    public ReactionType Type { get; private set; }
    public static Result<Reaction> Create(long postId, string userId, ReactionType type, DateTimeOffset? createdAt = null)
    {
        var errors = Validate(postId, userId, type);

        if (errors.Count > 0)
        {
            return Result<Reaction>.Failure(errors);
        }

        return Result<Reaction>.Success(new Reaction
        {
            PostId = postId,
            UserId = userId.Trim(),
            Type = type,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
        });
    }

    public Result ChangeTo(ReactionType type, DateTimeOffset? updatedAt = null)
    {
        if (!Enum.IsDefined(type))
        {
            return Result.Failure(new DomainError("Reaction.InvalidType", "La reaccion seleccionada no es valida."));
        }

        if (Type == type)
        {
            return Result.Success();
        }

        Type = type;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;

        return Result.Success();
    }

    private static List<DomainError> Validate(long postId, string userId, ReactionType type)
    {
        var errors = new List<DomainError>();

        if (postId <= 0)
        {
            errors.Add(new DomainError("Reaction.InvalidPost", "La publicacion relacionada no es valida."));
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            errors.Add(new DomainError("Reaction.UserRequired", "El usuario de la reaccion es requerido."));
        }

        if (!Enum.IsDefined(type))
        {
            errors.Add(new DomainError("Reaction.InvalidType", "La reaccion seleccionada no es valida."));
        }

        return errors;
    }
}
