using LinkUpPro.Application.DTOs.Notification.Requests;
using LinkUpPro.Application.DTOs.Notification.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Application.ViewModels.NotificationViewModels;
using LinkUpPro.Application.ViewModels.Shared;
using LinkUpPro.Domain.Enums;
using LinkUpPro.WebApp.Extensions;
using LinkUpPro.WebApp.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.WebApp.Controllers;

/// <summary>
/// Controlador para la gestión de notificaciones:
/// listado, marcar leídas, marcar todas como leídas.
/// </summary>
[SessionAuthorize]
public class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;
    private readonly IFriendRequestService _friendRequestService;
    private readonly IPostService _postService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        IFriendRequestService friendRequestService,
        IPostService postService,
        ICurrentUserService currentUserService,
        ILogger<NotificationsController> logger
    )
        : base(currentUserService)
    {
        _notificationService = notificationService;
        _friendRequestService = friendRequestService;
        _postService = postService;
        _logger = logger;
    }

    // ====================== NAVIGATE FROM NOTIFICATION ======================

    [HttpGet]
    public async Task<IActionResult> Navigate(long id)
    {
        var userId = _currentUserService.UserId!;

        // Get notification and verify ownership
        var paged = await _notificationService.GetNotificationsAsync(userId, null, 1, int.MaxValue);
        var notif = paged.Items.FirstOrDefault(n => n.Id == id);
        if (notif is null)
        {
            ShowError("La notificación no fue encontrada.");
            return RedirectToAction(nameof(Index));
        }

        // Mark as read
        await _notificationService.MarkAsReadAsync(userId, new MarkAsReadRequest(id));

        // Determine destination
        switch (notif.RelatedEntityType)
        {
            case RelatedEntityType.Post:
            case RelatedEntityType.Comment:
                if (notif.RelatedEntityId.HasValue)
                {
                    var postResult = await _postService.GetByIdAsync(userId, notif.RelatedEntityId.Value);
                    if (postResult.IsSuccess)
                        return RedirectToAction("Index", "Home", new { highlightPostId = notif.RelatedEntityId.Value });
                    ShowError("El contenido relacionado con esta notificación ya no se encuentra disponible.");
                    return RedirectToAction(nameof(Index));
                }
                break;
            case RelatedEntityType.FriendRequest:
                return RedirectToAction("Index", "FriendRequests");
        }

        ShowError("El contenido relacionado con esta notificación ya no se encuentra disponible.");
        return RedirectToAction(nameof(Index));
    }

    // ====================== INDEX ======================

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 20, bool? unreadOnly = null)
    {
        var userId = _currentUserService.UserId!;

        var pagedResult = await _notificationService.GetNotificationsAsync(userId, unreadOnly, page, pageSize);
        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

        var items = pagedResult.Items.Select(MapToListItem).ToList();

        var vm = new NotificationListViewModel
        {
            Notifications = new PagedResultViewModel<NotificationListItemViewModel>
            {
                Items = items,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize,
                TotalItems = pagedResult.TotalCount
            },
            UnreadOnly = unreadOnly,
            UnreadNotificationsCount = unreadCount.Count
        };

        await this.PopulateBaseViewModelAsync(vm, _currentUserService, _friendRequestService, _notificationService);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Recent()
    {
        var userId = _currentUserService.UserId!;
        var pagedResult = await _notificationService.GetNotificationsAsync(userId, null, 1, 5);
        var items = pagedResult.Items.Select(MapToListItem).ToList();

        return PartialView("_NotificationDropdown", items);
    }

    // ====================== MARK AS READ ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        try
        {
            var result = await _notificationService.MarkAsReadAsync(
                _currentUserService.UserId!,
                new MarkAsReadRequest(id)
            );

            if (!result.IsSuccess)
            {
                ShowError(result.Error?.Message ?? "No se pudo marcar la notificación como leída.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error marcando notificación {Id} como leída", id);
            ShowError("No se pudo marcar la notificación como leída.");
        }

        return RedirectToAction(nameof(Index));
    }

    // ====================== MARK ALL AS READ ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var result = await _notificationService.MarkAllAsReadAsync(_currentUserService.UserId!);
            if (!result.IsSuccess)
            {
                ShowError(result.Error?.Message ?? "No se pudieron marcar las notificaciones como leídas.");
            }
            else
            {
                ShowAlert("Todas las notificaciones marcadas como leídas.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error marcando todas las notificaciones como leídas");
            ShowError("No se pudieron marcar las notificaciones como leídas.");
        }

        return RedirectToAction(nameof(Index));
    }

    // ====================== PRIVATE MAPPER ======================

    private NotificationListItemViewModel MapToListItem(NotificationResponseDto dto)
    {
        string? actionUrl = null;
        if (dto.RelatedEntityId.HasValue)
        {
            switch (dto.RelatedEntityType)
            {
                case RelatedEntityType.Post:
                case RelatedEntityType.Comment:
                    actionUrl = Url.Action("Index", "Home", new { highlightPostId = dto.RelatedEntityId.Value });
                    break;
                case RelatedEntityType.FriendRequest:
                    actionUrl = Url.Action("Index", "FriendRequests");
                    break;
                default:
                    actionUrl = Url.Action("Index", "Home", new { highlightPostId = dto.RelatedEntityId.Value });
                    break;
            }
        }

        return new NotificationListItemViewModel
        {
            Id = dto.Id,
            ActorId = dto.ActorId,
            ActorName = dto.ActorName,
            ActorProfilePicture = dto.ActorProfilePicture,
            Type = dto.Type,
            Message = dto.Message,
            CreatedAt = dto.CreatedAt.UtcDateTime,
            IsRead = dto.IsRead,
            RelatedEntityType = dto.RelatedEntityType,
            RelatedEntityId = dto.RelatedEntityId,
            ActionUrl = actionUrl
        };
    }
}
