using LinkUpPro.Application.DTOs.FriendRequest.Requests;
using LinkUpPro.Application.DTOs.FriendRequest.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Application.ViewModels.FriendRequestViewModels;
using LinkUpPro.Application.ViewModels.Shared;
using LinkUpPro.Domain.Enums;
using LinkUpPro.WebApp.Extensions;
using LinkUpPro.WebApp.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.WebApp.Controllers;

/// <summary>
/// Controlador para gestión de solicitudes de amistad:
/// Listado de pendientes, enviadas, aceptar, rechazar, cancelar, eliminar historial, enviar nueva.
/// </summary>
[SessionAuthorize]
public class FriendRequestsController : BaseController
{
    private readonly IFriendRequestService _friendRequestService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<FriendRequestsController> _logger;

    public FriendRequestsController(
        IFriendRequestService friendRequestService,
        INotificationService notificationService,
        ICurrentUserService currentUserService,
        ILogger<FriendRequestsController> logger
    )
        : base(currentUserService)
    {
        _friendRequestService = friendRequestService;
        _notificationService = notificationService;
        _logger = logger;
    }

    // ====================== INDEX (Dashboard) ======================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _currentUserService.UserId!;

        var pending = await _friendRequestService.GetPendingRequestsAsync(userId, 1, 50);
        var sent = await _friendRequestService.GetSentRequestsAsync(userId, 1, 50);

        var pendingVms = pending.Items.Select(MapToPendingRequest).ToList();
        var sentVms = sent.Items.Select(MapToSentRequest).ToList();

        var vm = new FriendRequestHistoryViewModel
        {
            PendingRequests = pendingVms,
            SentRequests = sentVms
        };

        await this.PopulateBaseViewModelAsync(vm, _currentUserService, _friendRequestService, _notificationService);
        return View(vm);
    }

    // ====================== ACCEPT ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(long id)
    {
        try
        {
            var result = await _friendRequestService.AcceptAsync(
                _currentUserService.UserId!,
                new AcceptFriendRequestRequest(id)
            );

            if (!result.IsSuccess)
                ShowError(result.Error?.Message ?? "No se pudo aceptar la solicitud.");
            else
                ShowAlert("La solicitud fue aceptada correctamente.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error aceptando solicitud {Id}", id);
            ShowError("No se pudo aceptar la solicitud.");
        }

        return RedirectToAction(nameof(Index));
    }

    // ====================== REJECT ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(long id)
    {
        try
        {
            var result = await _friendRequestService.RejectAsync(
                _currentUserService.UserId!,
                new RejectFriendRequestRequest(id)
            );

            if (!result.IsSuccess)
                ShowError(result.Error?.Message ?? "No se pudo rechazar la solicitud.");
            else
                ShowAlert("La solicitud fue rechazada correctamente.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error rechazando solicitud {Id}", id);
            ShowError("No se pudo rechazar la solicitud.");
        }

        return RedirectToAction(nameof(Index));
    }

    // ====================== CANCEL ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(long id)
    {
        try
        {
            var result = await _friendRequestService.CancelAsync(
                _currentUserService.UserId!,
                new CancelFriendRequestRequest(id)
            );

            if (!result.IsSuccess)
                ShowError(result.Error?.Message ?? "No se pudo cancelar la solicitud.");
            else
                ShowAlert("La solicitud fue cancelada correctamente.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cancelando solicitud {Id}", id);
            ShowError("No se pudo cancelar la solicitud.");
        }

        return RedirectToAction(nameof(Index));
    }

    // ====================== DELETE FROM HISTORY ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFromHistory(long id)
    {
        try
        {
            var result = await _friendRequestService.DeleteFromHistoryAsync(
                _currentUserService.UserId!,
                new DeleteFromHistoryRequest(id)
            );

            if (!result.IsSuccess)
                ShowError(result.Error?.Message ?? "No se pudo eliminar la solicitud del historial.");
            else
                ShowAlert("La solicitud fue eliminada de su historial.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error eliminando solicitud {Id}", id);
            ShowError("No se pudo eliminar la solicitud del historial.");
        }

        return RedirectToAction(nameof(Index));
    }

    // ====================== SEARCH USERS (AJAX - partial view) ======================

    [HttpGet]
    public async Task<IActionResult> SearchUsers(string search, int page = 1, int pageSize = 20)
    {
        var userId = _currentUserService.UserId!;
        var pagedResult = await _friendRequestService.SearchAvailableUsersAsync(userId, search, page, pageSize);

        var items = pagedResult.Items.Select(MapToAvailableUser).ToList();

        var vm = new SendFriendRequestViewModel
        {
            SearchText = search,
            IsSearching = !string.IsNullOrWhiteSpace(search),
            AvailableUsers = items
        };

        return PartialView("_AvailableUsersPartial", vm);
    }

    // ====================== SEND REQUEST (GET - buscar usuarios) ======================

    [HttpGet]
    public async Task<IActionResult> SendRequest(string? search)
    {
        var userId = _currentUserService.UserId!;
        var pagedResult = await _friendRequestService.SearchAvailableUsersAsync(userId, search, 1, 50);

        var vm = new SendFriendRequestViewModel
        {
            SearchText = search,
            IsSearching = !string.IsNullOrWhiteSpace(search),
            AvailableUsers = pagedResult.Items.Select(MapToAvailableUser).ToList()
        };

        await this.PopulateMenuCountersAsync(_currentUserService, _friendRequestService, _notificationService);
        return View(vm);
    }

    // ====================== SEND REQUEST (POST) ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendRequest(SendFriendRequestViewModel model)
    {
        if (string.IsNullOrEmpty(model.SelectedUserId))
        {
            ModelState.AddModelError(string.Empty, "Debe seleccionar un usuario para enviar la solicitud de amistad.");
            return await SendRequest(model.SearchText);
        }

        try
        {
            var result = await _friendRequestService.SendAsync(
                _currentUserService.UserId!,
                new SendFriendRequestRequest(model.SelectedUserId)
            );

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error?.Message ?? "No se pudo enviar la solicitud.");
                return await SendRequest(model.SearchText);
            }

            ShowAlert("La solicitud de amistad fue enviada correctamente.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error enviando solicitud a {UserId}", model.SelectedUserId);
            ModelState.AddModelError(string.Empty, "No se pudo enviar la solicitud.");
            return await SendRequest(model.SearchText);
        }
    }

    // ====================== PRIVATE MAPPERS ======================

    private PendingRequestViewModel MapToPendingRequest(PendingRequestDto dto)
    {
        return new PendingRequestViewModel
        {
            RequestId = dto.Id,
            SenderId = dto.SenderId,
            SenderName = dto.SenderName,
            SenderUserName = dto.SenderUserName,
            SenderProfilePicture = dto.SenderProfilePicture,
            CommonFriendsCount = dto.CommonFriendsCount,
            SentAt = dto.SentAt.UtcDateTime
        };
    }

    private SentRequestViewModel MapToSentRequest(SentRequestDto dto)
    {
        return new SentRequestViewModel
        {
            RequestId = dto.Id,
            ReceiverId = dto.ReceiverId,
            ReceiverName = dto.ReceiverName,
            ReceiverUserName = dto.ReceiverUserName,
            ReceiverProfilePicture = dto.ReceiverProfilePicture,
            CommonFriendsCount = dto.CommonFriendsCount,
            SentAt = dto.SentAt.UtcDateTime,
            Status = dto.Status,
            RespondedAt = dto.RespondedAt?.UtcDateTime,
            IsVisibleForSender = dto.IsVisibleForSender,
            SenderName = dto.SenderName
        };
    }

    private AvailableUserViewModel MapToAvailableUser(AvailableUserDto dto)
    {
        return new AvailableUserViewModel
        {
            UserId = dto.Id,
            Name = dto.Name,
            UserName = dto.UserName,
            ProfilePicturePath = dto.ProfilePicturePath,
            CommonFriendsCount = dto.CommonFriendsCount
        };
    }
}
