using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Application.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.WebApp.Extensions;

/// <summary>
/// Métodos de extensión para los Controladores de la capa de presentación.
/// Centraliza la población de los contadores del menú lateral (solicitudes, notificaciones)
/// y la información del usuario actual en el BaseViewModel.
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Puebla el BaseViewModel con la información del usuario actual
    /// y los contadores del menú superior (solicitudes pendientes, notificaciones no leídas).
    /// </summary>
    public static async Task PopulateBaseViewModelAsync(
        this Controller _controller,
        BaseViewModel viewModel,
        ICurrentUserService currentUserService,
        IFriendRequestService? friendRequestService = null,
        INotificationService? notificationService = null
    )
    {
        if (viewModel == null) return;

        viewModel.CurrentUserId = currentUserService.UserId ?? string.Empty;
        viewModel.CurrentUserFullName = currentUserService.FullName ?? "Usuario";
        viewModel.CurrentUserName = currentUserService.UserName ?? string.Empty;
        viewModel.CurrentUserProfilePicture = currentUserService.ProfilePicturePath;

        // ViewBag para partials que no tienen acceso al ViewModel tipado
        _controller.ViewBag.CurrentUserAvatar = currentUserService.ProfilePicturePath;
        _controller.ViewBag.CurrentUserFullName = currentUserService.FullName ?? "Usuario";

        if (string.IsNullOrEmpty(currentUserService.UserId))
            return;

        // Contadores secuenciales para evitar concurrencia en el mismo DbContext scoped
        viewModel.PendingFriendRequestsCount = friendRequestService is not null
            ? await friendRequestService.GetPendingCountAsync(currentUserService.UserId)
            : 0;

        var unreadCount = notificationService is not null
            ? await notificationService.GetUnreadCountAsync(currentUserService.UserId)
            : null;

        viewModel.UnreadNotificationsCount = unreadCount?.Count ?? 0;
    }

    /// <summary>
    /// Puebla el ViewBag con los contadores del menú superior (legacy - solo para vistas que no usan BaseViewModel).
    /// </summary>
    public static async Task PopulateMenuCountersAsync(
        this Controller controller,
        ICurrentUserService currentUserService,
        IFriendRequestService? friendRequestService = null,
        INotificationService? notificationService = null
    )
    {
        if (!currentUserService.IsAuthenticated || string.IsNullOrEmpty(currentUserService.UserId))
        {
            controller.ViewBag.PendingRequestsCount = 0;
            controller.ViewBag.UnreadNotificationsCount = 0;
            return;
        }

        var userId = currentUserService.UserId;
        controller.ViewBag.PendingRequestsCount = friendRequestService is not null
            ? await friendRequestService.GetPendingCountAsync(userId)
            : 0;

        var unreadCount = notificationService is not null
            ? await notificationService.GetUnreadCountAsync(userId)
            : null;

        controller.ViewBag.UnreadNotificationsCount = unreadCount?.Count ?? 0;
    }
}
