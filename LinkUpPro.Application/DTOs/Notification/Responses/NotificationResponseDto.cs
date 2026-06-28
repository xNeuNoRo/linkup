using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.DTOs.Notification.Responses;

public record NotificationResponseDto(
    long Id, string ActorId, string ActorName, string? ActorProfilePicture,
    NotificationType Type, string Message, DateTimeOffset CreatedAt, bool IsRead,
    long? RelatedEntityId);
