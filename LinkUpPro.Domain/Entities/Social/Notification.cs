using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Domain.Entities.Social;

public sealed class Notification : AuditableBaseEntity<long>
{
    // Tipos de notificación existentes
    public const string CommentType = "Comment";
    public const string ReplyType = "Reply";
    public const string ReactionTypeName = "Reaction";

    // Tipos de notificación de amistad
    public const string FriendRequestSentType = "FriendRequestSent";
    public const string FriendRequestAcceptedType = "FriendRequestAccepted";
    public const string FriendRequestRejectedType = "FriendRequestRejected";

    private Notification() { }

    public string RecipientId { get; private set; } = null!;

    public string ActorId { get; private set; } = null!;

    public string Type { get; private set; } = null!;

    public string Message { get; private set; } = null!;

    public long? RelatedEntityId { get; private set; }

    public bool IsRead { get; private set; }

    public static Result<Notification> CreateComment(
        string recipientId,
        string actorId,
        long postId,
        string actorUserName,
        DateTimeOffset? createdAt = null
    ) =>
        Create(
            recipientId,
            actorId,
            CommentType,
            $"{actorUserName} comento tu publicacion.",
            postId,
            createdAt
        );

    public static Result<Notification> CreateReply(
        string recipientId,
        string actorId,
        long postId,
        string actorUserName,
        DateTimeOffset? createdAt = null
    ) =>
        Create(
            recipientId,
            actorId,
            ReplyType,
            $"{actorUserName} respondio tu comentario.",
            postId,
            createdAt
        );

    public static Result<Notification> CreateReaction(
        string recipientId,
        string actorId,
        long postId,
        string actorUserName,
        Enums.ReactionType reactionType,
        DateTimeOffset? createdAt = null
    ) =>
        Create(
            recipientId,
            actorId,
            ReactionTypeName,
            $"{actorUserName} reacciono con {GetReactionDisplayName(reactionType)} a tu publicacion.",
            postId,
            createdAt
        );

    public void MarkAsRead(DateTimeOffset? updatedAt = null)
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
    }

    public bool IsForUser(string userId) => RecipientId == userId;

    public static Result<Notification> Create(
        string recipientId,
        string actorId,
        string type,
        string message,
        long? relatedEntityId = null,
        DateTimeOffset? createdAt = null
    )
    {
        var errors = new List<DomainError>();

        if (string.IsNullOrWhiteSpace(recipientId))
        {
            errors.Add(
                new DomainError(
                    "Notification.RecipientRequired",
                    "El destinatario de la notificacion es requerido."
                )
            );
        }

        if (string.IsNullOrWhiteSpace(actorId))
        {
            errors.Add(
                new DomainError(
                    "Notification.ActorRequired",
                    "El actor de la notificacion es requerido."
                )
            );
        }

        if (recipientId == actorId)
        {
            errors.Add(
                new DomainError(
                    "Notification.SelfNotificationNotAllowed",
                    "No se generan notificaciones para acciones propias."
                )
            );
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            errors.Add(
                new DomainError(
                    "Notification.TypeRequired",
                    "El tipo de notificacion es requerido."
                )
            );
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            errors.Add(
                new DomainError(
                    "Notification.MessageRequired",
                    "El mensaje de la notificacion es requerido."
                )
            );
        }

        if (errors.Count > 0)
        {
            return Result<Notification>.Failure(errors);
        }

        return Result<Notification>.Success(
            new Notification
            {
                RecipientId = recipientId.Trim(),
                ActorId = actorId.Trim(),
                Type = type.Trim(),
                Message = message.Trim(),
                RelatedEntityId = relatedEntityId,
                IsRead = false,
                CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
            }
        );
    }

    private static string GetReactionDisplayName(Enums.ReactionType reactionType) =>
        reactionType switch
        {
            Enums.ReactionType.Like => "Me gusta",
            Enums.ReactionType.Dislike => "No me gusta",
            _ => "una reaccion",
        };
}
