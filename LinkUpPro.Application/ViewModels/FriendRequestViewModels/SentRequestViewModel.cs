using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.ViewModels.FriendRequestViewModels;

/// <summary>
/// ViewModel para solicitudes enviadas (historial del emisor).
/// Puede estar en estado Pendiente, Aceptada o Rechazada.
/// </summary>
public class SentRequestViewModel
{
    public long RequestId { get; set; }
    public string ReceiverId { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverUserName { get; set; } = string.Empty;
    public string? ReceiverProfilePicture { get; set; }
    public int CommonFriendsCount { get; set; }
    public DateTime SentAt { get; set; }
    public FriendRequestStatus Status { get; set; }
    public DateTime? RespondedAt { get; set; }

    /// <summary>
    /// Indica si la solicitud sigue siendo visible en el historial del emisor.
    /// El emisor puede ocultarla (solo cambia este flag, no el estado).
    /// </summary>
    public bool IsVisibleForSender { get; set; } = true;

    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// Etiqueta legible del estado para mostrar en la UI.
    /// </summary>
    public string StatusLabel => Status switch
    {
        FriendRequestStatus.Pending => "En espera de respuesta",
        FriendRequestStatus.Accepted => "Aceptada",
        FriendRequestStatus.Rejected => "Rechazada",
        FriendRequestStatus.Canceled => "Cancelada",
        _ => "Desconocido"
    };
}
