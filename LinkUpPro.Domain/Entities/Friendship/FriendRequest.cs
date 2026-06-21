using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Domain.Entities.Friendship;

public sealed class FriendRequest : BaseEntity<long>
{
    private FriendRequest()
    {
    }

    public string SenderId { get; private set; } = null!;

    public string ReceiverId { get; private set; } = null!;

    public FriendRequestStatus Status { get; private set; }

    public DateTimeOffset SentAt { get; private set; }

    public DateTimeOffset? RespondedAt { get; private set; }

    public bool IsVisibleForSender { get; private set; }
    public static Result<FriendRequest> Create(string senderId, string receiverId, DateTimeOffset? sentAt = null)
    {
        var errors = ValidateUsers(senderId, receiverId);

        if (errors.Count > 0)
        {
            return Result<FriendRequest>.Failure(errors);
        }

        var date = sentAt ?? DateTimeOffset.UtcNow;

        return Result<FriendRequest>.Success(new FriendRequest
        {
            SenderId = senderId.Trim(),
            ReceiverId = receiverId.Trim(),
            Status = FriendRequestStatus.Pending,
            SentAt = date,
            CreatedAt = date,
            IsVisibleForSender = true,
        });
    }

    public Result Accept(string receiverId, DateTimeOffset? respondedAt = null)
    {
        var authorization = EnsureCanBeRespondedBy(receiverId, "FriendRequest.AcceptNotAllowed", "No posee permisos para aceptar esta solicitud.");

        if (authorization.IsFailure)
        {
            return authorization;
        }

        SetStatus(FriendRequestStatus.Accepted, respondedAt);
        return Result.Success();
    }

    public Result Reject(string receiverId, DateTimeOffset? respondedAt = null)
    {
        var authorization = EnsureCanBeRespondedBy(receiverId, "FriendRequest.RejectNotAllowed", "No posee permisos para rechazar esta solicitud.");

        if (authorization.IsFailure)
        {
            return authorization;
        }

        SetStatus(FriendRequestStatus.Rejected, respondedAt);
        return Result.Success();
    }

    public Result Cancel(string senderId, DateTimeOffset? canceledAt = null)
    {
        if (SenderId != senderId)
        {
            return Result.Failure(new DomainError(
                "FriendRequest.CancelNotAllowed",
                "No posee permisos para cancelar esta solicitud."));
        }

        if (Status != FriendRequestStatus.Pending)
        {
            return Result.Failure(new DomainError(
                "FriendRequest.NotPending",
                "Esta solicitud ya no se encuentra disponible."));
        }

        SetStatus(FriendRequestStatus.Canceled, canceledAt);
        return Result.Success();
    }

    public Result HideFromSenderHistory(string senderId, DateTimeOffset? updatedAt = null)
    {
        if (SenderId != senderId)
        {
            return Result.Failure(new DomainError(
                "FriendRequest.HideNotAllowed",
                "No posee permisos para eliminar esta solicitud del historial."));
        }

        if (Status is not (FriendRequestStatus.Accepted or FriendRequestStatus.Rejected))
        {
            return Result.Failure(new DomainError(
                "FriendRequest.InvalidStatusForHistoryHide",
                "Solo las solicitudes aceptadas o rechazadas pueden eliminarse del historial."));
        }

        if (!IsVisibleForSender)
        {
            return Result.Success();
        }

        IsVisibleForSender = false;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public bool IsBetween(string firstUserId, string secondUserId) =>
        (SenderId == firstUserId && ReceiverId == secondUserId) ||
        (SenderId == secondUserId && ReceiverId == firstUserId);

    public bool CanBeAcceptedBy(string userId) => ReceiverId == userId && Status == FriendRequestStatus.Pending;

    public bool CanBeRejectedBy(string userId) => CanBeAcceptedBy(userId);

    public bool CanBeCanceledBy(string userId) => SenderId == userId && Status == FriendRequestStatus.Pending;

    public bool CanBeHiddenBy(string userId) =>
        SenderId == userId && Status is FriendRequestStatus.Accepted or FriendRequestStatus.Rejected;

    private Result EnsureCanBeRespondedBy(string receiverId, string code, string message)
    {
        if (ReceiverId != receiverId)
        {
            return Result.Failure(new DomainError(code, message));
        }

        if (Status != FriendRequestStatus.Pending)
        {
            return Result.Failure(new DomainError(
                "FriendRequest.NotPending",
                "Esta solicitud ya no se encuentra disponible."));
        }

        return Result.Success();
    }

    private void SetStatus(FriendRequestStatus status, DateTimeOffset? timestamp)
    {
        var date = timestamp ?? DateTimeOffset.UtcNow;
        Status = status;
        RespondedAt = date;
        UpdatedAt = date;
    }

    private static List<DomainError> ValidateUsers(string senderId, string receiverId)
    {
        var errors = new List<DomainError>();

        if (string.IsNullOrWhiteSpace(senderId))
        {
            errors.Add(new DomainError("FriendRequest.SenderRequired", "El emisor de la solicitud es requerido."));
        }

        if (string.IsNullOrWhiteSpace(receiverId))
        {
            errors.Add(new DomainError("FriendRequest.ReceiverRequired", "El receptor de la solicitud es requerido."));
        }

        if (!string.IsNullOrWhiteSpace(senderId) && senderId.Trim() == receiverId?.Trim())
        {
            errors.Add(new DomainError("FriendRequest.SelfRequestNotAllowed", "Un usuario no puede enviarse una solicitud a si mismo."));
        }

        return errors;
    }
}
