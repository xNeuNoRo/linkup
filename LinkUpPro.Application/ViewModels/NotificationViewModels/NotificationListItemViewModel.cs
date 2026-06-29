using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.ViewModels.NotificationViewModels;

/// <summary>
/// ViewModel para mostrar una notificación individual en el listado.
/// Mapea desde NotificationResponseDto.
/// </summary>
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

    /// <summary>
    /// ID de la entidad relacionada (PostId en la mayoría de los casos).
    /// </summary>
    public long? RelatedEntityId { get; set; }

    /// <summary>
    /// URL a la que debe redirigirse al hacer click (post, comentario, etc.).
    /// </summary>
    public string? ActionUrl { get; set; }
}
