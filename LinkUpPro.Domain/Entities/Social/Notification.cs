using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Domain.Entities.Social;

public sealed class Notification : AuditableBaseEntity<long>
{
    private Notification() { }

    public string RecipientId { get; private set; } = null!;

    public string ActorId { get; private set; } = null!;

    public NotificationType Type { get; private set; }

    public string Message { get; private set; } = null!;

    public long? RelatedEntityId { get; private set; }

    public RelatedEntityType RelatedEntityType { get; private set; }

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
            NotificationType.Comment,
            $"{actorUserName} comento tu publicacion.",
            RelatedEntityType.Post,
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
            NotificationType.Reply,
            $"{actorUserName} respondio tu comentario.",
            RelatedEntityType.Post,
            postId,
            createdAt
        );

    public static Result<Notification> CreateReaction(
        string recipientId,
        string actorId,
        long postId,
        string actorUserName,
        ReactionType reactionType,
        DateTimeOffset? createdAt = null
    ) =>
        Create(
            recipientId,
            actorId,
            NotificationType.Reaction,
            $"{actorUserName} reacciono con {GetReactionDisplayName(reactionType)} a tu publicacion.",
            RelatedEntityType.Post,
            postId,
            createdAt
        );

    public static Result<Notification> CreateReactionChange(
        string recipientId,
        string actorId,
        long postId,
        string actorUserName,
        ReactionType oldReactionType,
        ReactionType newReactionType,
        DateTimeOffset? createdAt = null
    ) =>
        Create(
            recipientId,
            actorId,
            NotificationType.ReactionChange,
            $"{actorUserName} cambio su reaccion de {GetReactionDisplayName(oldReactionType)} a {GetReactionDisplayName(newReactionType)} en tu publicacion.",
            RelatedEntityType.Post,
            postId,
            createdAt
        );

    public static Result<Notification> CreateFriendRequestSent(
        string recipientId,
        string actorId,
        long requestId,
        string actorUserName,
        DateTimeOffset? createdAt = null
    ) =>
        Create(
            recipientId,
            actorId,
            NotificationType.FriendRequestSent,
            $"{actorUserName} te envio una solicitud de amistad.",
            RelatedEntityType.FriendRequest,
            requestId,
            createdAt
        );

    public static Result<Notification> CreateFriendRequestAccepted(
        string recipientId,
        string actorId,
        long requestId,
        string actorUserName,
        DateTimeOffset? createdAt = null
    ) =>
        Create(
            recipientId,
            actorId,
            NotificationType.FriendRequestAccepted,
            $"{actorUserName} acepto tu solicitud de amistad.",
            RelatedEntityType.FriendRequest,
            requestId,
            createdAt
        );

    public static Result<Notification> CreateBattleshipGameInvited(
        string recipientId,
        string actorId,
        long gameId,
        string actorUserName,
        DateTimeOffset? createdAt = null
    ) =>
        Create(
            recipientId,
            actorId,
            NotificationType.BattleshipGameInvited,
            $"{actorUserName} te invito a una partida de Battleship.",
            RelatedEntityType.BattleshipGame,
            gameId,
            createdAt
        );

    public static Result<Notification> CreateBattleshipGameStarted(
        string recipientId,
        string actorId,
        long gameId,
        string actorUserName,
        DateTimeOffset? createdAt = null
    ) =>
        Create(
            recipientId,
            actorId,
            NotificationType.BattleshipGameStarted,
            $"{actorUserName} coloco todos sus barcos. ¡La partida comienza!",
            RelatedEntityType.BattleshipGame,
            gameId,
            createdAt
        );

    public static Result<Notification> CreateBattleshipShipSunk(
        string recipientId,
        string actorId,
        long gameId,
        int shipSize,
        string actorUserName,
        DateTimeOffset? createdAt = null
    ) =>
        Create(
            recipientId,
            actorId,
            NotificationType.BattleshipShipSunk,
            $"{actorUserName} hundio tu barco de {shipSize} posiciones.",
            RelatedEntityType.BattleshipGame,
            gameId,
            createdAt
        );

    public static Result<Notification> CreateBattleshipShipSunkByOpponent(
        string recipientId,
        string actorId,
        long gameId,
        int shipSize,
        string actorUserName,
        DateTimeOffset? createdAt = null
    ) =>
        Create(
            recipientId,
            actorId,
            NotificationType.BattleshipShipSunkByOpponent,
            $"{actorUserName} ¡Hundiste un barco de {shipSize} posiciones!",
            RelatedEntityType.BattleshipGame,
            gameId,
            createdAt
        );

    public static Result<Notification> CreateFriendRequestRejected(
        string recipientId,
        string actorId,
        long requestId,
        string actorUserName,
        DateTimeOffset? createdAt = null
    ) =>
        Create(
            recipientId,
            actorId,
            NotificationType.FriendRequestRejected,
            $"{actorUserName} rechazo tu solicitud de amistad.",
            RelatedEntityType.FriendRequest,
            requestId,
            createdAt
        );

    public void MarkAsRead(DateTimeOffset? updatedAt = null)
    {
        if (IsRead)
            return;

        IsRead = true;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
    }

    public bool IsForUser(string userId) => RecipientId == userId;

    public static Result<Notification> Create(
        string recipientId,
        string actorId,
        NotificationType type,
        string message,
        RelatedEntityType entityType = RelatedEntityType.None,
        long? relatedEntityId = null,
        DateTimeOffset? createdAt = null
    )
    {
        var errors = new List<DomainError>();

        if (string.IsNullOrWhiteSpace(recipientId))
            errors.Add(new DomainError("Notification.RecipientRequired", "El destinatario de la notificacion es requerido."));

        if (string.IsNullOrWhiteSpace(actorId))
            errors.Add(new DomainError("Notification.ActorRequired", "El actor de la notificacion es requerido."));

        if (recipientId == actorId)
            errors.Add(new DomainError("Notification.SelfNotificationNotAllowed", "No se generan notificaciones para acciones propias."));

        if (!Enum.IsDefined(type))
            errors.Add(new DomainError("Notification.InvalidType", "El tipo de notificacion no es valido."));

        if (string.IsNullOrWhiteSpace(message))
            errors.Add(new DomainError("Notification.MessageRequired", "El mensaje de la notificacion es requerido."));

        if (relatedEntityId.HasValue && entityType == RelatedEntityType.None)
            errors.Add(new DomainError("Notification.EntityTypeRequired", "El tipo de entidad relacionada es requerido cuando se especifica un ID."));

        if (errors.Count > 0)
            return Result<Notification>.Failure(errors);

        return Result<Notification>.Success(
            new Notification
            {
                RecipientId = recipientId.Trim(),
                ActorId = actorId.Trim(),
                Type = type,
                Message = message.Trim(),
                RelatedEntityType = entityType,
                RelatedEntityId = relatedEntityId,
                IsRead = false,
                CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
            }
        );
    }

    private static string GetReactionDisplayName(ReactionType reactionType) =>
        reactionType switch
        {
            ReactionType.Like => "Me gusta",
            ReactionType.Dislike => "No me gusta",
            _ => "una reaccion",
        };
}
