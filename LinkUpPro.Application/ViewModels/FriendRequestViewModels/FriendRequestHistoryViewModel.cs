using LinkUpPro.Application.ViewModels.Shared;

namespace LinkUpPro.Application.ViewModels.FriendRequestViewModels;

/// <summary>
/// ViewModel principal de la pantalla "Solicitudes de amistad".
/// Combina solicitudes pendientes (recibidas) y solicitudes enviadas (historial).
/// Hereda de BaseViewModel para incluir el contador del menú.
/// </summary>
public class FriendRequestHistoryViewModel : BaseViewModel
{
    public List<PendingRequestViewModel> PendingRequests { get; set; } = [];
    public List<SentRequestViewModel> SentRequests { get; set; } = [];
}
