using LinkUpPro.Application.ViewModels.Shared;

namespace LinkUpPro.Application.ViewModels.NotificationViewModels;

/// <summary>
/// ViewModel principal de la pantalla "Notificaciones".
/// Incluye la lista paginada de notificaciones y el contador de no leídas.
/// Hereda de BaseViewModel para mantener el contador actualizado en el menú.
/// </summary>
public class NotificationListViewModel : BaseViewModel
{
    public PagedResultViewModel<NotificationListItemViewModel> Notifications { get; set; } = new();

    /// <summary>
    /// Filtro: null = todas, true = solo no leídas, false = solo leídas.
    /// </summary>
    public bool? UnreadOnly { get; set; }
}
