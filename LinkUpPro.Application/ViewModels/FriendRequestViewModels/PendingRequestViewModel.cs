namespace LinkUpPro.Application.ViewModels.FriendRequestViewModels;

/// <summary>
/// ViewModel para solicitudes pendientes recibidas (sección "Solicitudes de amistad").
/// Solo se muestran solicitudes en estado "En espera de respuesta".
/// </summary>
public class PendingRequestViewModel
{
    public long RequestId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderUserName { get; set; } = string.Empty;
    public string? SenderProfilePicture { get; set; }
    public int CommonFriendsCount { get; set; }
    public DateTime SentAt { get; set; }
}
