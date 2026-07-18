namespace LinkUpPro.Application.ViewModels.Shared;

/// <summary>
/// ViewModel base con datos comunes a todas las vistas autenticadas
/// (información del usuario logueado, contadores del menú superior).
/// </summary>
public abstract class BaseViewModel
{
    /// <summary>
    /// Identificador del usuario autenticado actualmente.
    /// </summary>
    public string CurrentUserId { get; set; } = string.Empty;

    /// <summary>
    /// Nombre completo del usuario logueado (para el menú).
    /// </summary>
    public string CurrentUserFullName { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de usuario (para el menú).
    /// </summary>
    public string CurrentUserName { get; set; } = string.Empty;

    /// <summary>
    /// Ruta de la foto de perfil del usuario logueado.
    /// </summary>
    public string? CurrentUserProfilePicture { get; set; }

    /// <summary>
    /// Contador de solicitudes de amistad pendientes recibidas (para badge del menú).
    /// </summary>
    public int PendingFriendRequestsCount { get; set; }

    /// <summary>
    /// Contador de notificaciones no leídas (para badge del menú).
    /// </summary>
    public int UnreadNotificationsCount { get; set; }
}
