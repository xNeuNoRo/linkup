using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.ViewModels.NotificationViewModels;

public class NotificationListItemViewModel
{
    public long Id { get; set; }
    public string ActorId { get; set; } = string.Empty;
    public string ActorName { get; set; } = string.Empty;
    public string? ActorProfilePicture { get; set; }

    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }

    public RelatedEntityType RelatedEntityType { get; set; }

    public long? RelatedEntityId { get; set; }

    public string? ActionUrl { get; set; }
}
