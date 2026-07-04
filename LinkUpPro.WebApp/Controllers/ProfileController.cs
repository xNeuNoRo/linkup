using LinkUpPro.Application.DTOs.Profile.Requests;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Application.ViewModels.FriendshipViewModels;
using LinkUpPro.Application.ViewModels.PostViewModels;
using LinkUpPro.Application.ViewModels.ProfileViewModels;
using LinkUpPro.Application.ViewModels.Shared;
using LinkUpPro.WebApp.Extensions;
using LinkUpPro.WebApp.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.WebApp.Controllers;

/// <summary>
/// Controlador de "Mi Perfil":
/// - Ver perfil (read-only)
/// - Editar perfil (nombre, apellido, teléfono, foto)
/// - Cambiar contraseña
/// </summary>
[SessionAuthorize]
public class ProfileController : BaseController
{
    private readonly IProfileService _profileService;
    private readonly IFriendRequestService _friendRequestService;
    private readonly INotificationService _notificationService;
    private readonly IPostService _postService;
    private readonly IReactionService _reactionService;
    private readonly IFriendshipService _friendshipService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IProfileService profileService,
        IFriendRequestService friendRequestService,
        INotificationService notificationService,
        IPostService postService,
        IReactionService reactionService,
        IFriendshipService friendshipService,
        ICurrentUserService currentUserService,
        ILogger<ProfileController> logger
    )
        : base(currentUserService)
    {
        _profileService = profileService;
        _friendRequestService = friendRequestService;
        _notificationService = notificationService;
        _postService = postService;
        _reactionService = reactionService;
        _friendshipService = friendshipService;
        _logger = logger;
    }

    // ====================== INDEX (Ver perfil) ======================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _currentUserService.UserId!;
        var result = await _profileService.GetProfileAsync(userId);

        if (!result.IsSuccess)
        {
            ShowError("No se pudo cargar el perfil.");
            return RedirectToAction("Index", "Home");
        }

        var dto = result.Value!;
        var statsResult = await _postService.GetStatsAsync(userId);
        var stats = statsResult.IsSuccess ? statsResult.Value : null;

        var vm = new ProfileViewModel
        {
            Id = dto.Id,
            UserName = dto.UserName,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            ProfilePicturePath = dto.ProfilePicturePath,
            IsActive = dto.IsActive,
            IsVerified = dto.IsVerified,
            CreatedAt = dto.CreatedAt,
            LastActivityAt = dto.LastActivityAt,
            TotalPosts = stats?.TotalPosts ?? 0,
            ImagePosts = stats?.ImagePosts ?? 0,
            VideoPosts = stats?.VideoPosts ?? 0,
            FriendsOnlyPosts = stats?.FriendsOnlyPosts ?? 0,
            OnlyMePosts = stats?.OnlyMePosts ?? 0,
            EditedPosts = stats?.EditedPosts ?? 0,
        };

        await this.PopulateMenuCountersAsync(
            _currentUserService,
            _friendRequestService,
            _notificationService
        );
        return View(vm);
    }

    // ====================== EDIT ======================

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var userId = _currentUserService.UserId!;
        var result = await _profileService.GetProfileAsync(userId);

        if (!result.IsSuccess)
        {
            ShowError("No se pudo cargar el perfil.");
            return RedirectToAction(nameof(Index));
        }

        var dto = result.Value!;
        var vm = new EditProfileViewModel
        {
            UpdateProfile = new UpdateProfileViewModel
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                CurrentProfilePicturePath = dto.ProfilePicturePath,
            },
        };

        await this.PopulateMenuCountersAsync(
            _currentUserService,
            _friendRequestService,
            _notificationService
        );
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await this.PopulateMenuCountersAsync(
                _currentUserService,
                _friendRequestService,
                _notificationService
            );
            return View(model);
        }

        try
        {
            var request = new UpdateProfileRequest(
                model.UpdateProfile.FirstName,
                model.UpdateProfile.LastName,
                model.UpdateProfile.PhoneNumber,
                model.ProfilePictureFile
            );

            var result = await _profileService.UpdateProfileAsync(
                _currentUserService.UserId!,
                request
            );
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Error?.Message ?? "No se pudo actualizar el perfil."
                );
                await this.PopulateMenuCountersAsync(
                    _currentUserService,
                    _friendRequestService,
                    _notificationService
                );
                return View(model);
            }

            ShowAlert("Su perfil fue actualizado correctamente.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error actualizando perfil");
            ModelState.AddModelError(string.Empty, "No se pudo actualizar el perfil.");
            await this.PopulateMenuCountersAsync(
                _currentUserService,
                _friendRequestService,
                _notificationService
            );
            return View(model);
        }
    }

    // ====================== CHANGE PASSWORD ======================

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await this.PopulateMenuCountersAsync(
                _currentUserService,
                _friendRequestService,
                _notificationService
            );
            return View(model);
        }

        var anyField =
            !string.IsNullOrWhiteSpace(model.CurrentPassword)
            || !string.IsNullOrWhiteSpace(model.NewPassword)
            || !string.IsNullOrWhiteSpace(model.ConfirmPassword);

        if (!anyField)
        {
            ShowInfo("No se proporcionó contraseña. El perfil se mantiene sin cambios.");
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var request = new ChangePasswordRequest(
                model.CurrentPassword,
                model.NewPassword,
                model.ConfirmPassword
            );

            var result = await _profileService.ChangePasswordAsync(
                _currentUserService.UserId!,
                request
            );
            if (!result.IsSuccess)
            {
                ShowError(result.Error?.Message ?? "No se pudo cambiar la contraseña.");
                await this.PopulateMenuCountersAsync(
                    _currentUserService,
                    _friendRequestService,
                    _notificationService
                );
                return View(model);
            }

            if (result.Value!.RequiresReLogin)
            {
                ShowAlert(
                    "Su perfil y contraseña fueron actualizados correctamente. Inicie sesión nuevamente."
                );
                // El SignOut ya fue hecho en el servicio, solo redirigir al login
                return RedirectToAction("Login", "Auth");
            }

            ShowAlert("Su contraseña fue actualizada correctamente.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cambiando contraseña");
            ShowError("No se pudo cambiar la contraseña.");
            return View(model);
        }
    }

    // ====================== VER PERFIL DE OTRO USUARIO (público desde amigos) ======================

    [HttpGet]
    public async Task<IActionResult> ViewProfile(string id, PostFilterViewModel filter)
    {
        if (id == _currentUserService.UserId)
            return RedirectToAction(nameof(Index));

        var friendship = await _friendshipService.GetFriendshipAsync(
            _currentUserService.UserId!,
            id
        );
        if (!friendship.IsSuccess)
        {
            ShowError("No posee permisos para visualizar el perfil de este usuario.");
            return RedirectToAction("Index", "Friends");
        }

        var profileResult = await _profileService.GetByIdAsync(id);
        if (profileResult == null)
        {
            ShowError("El usuario no fue encontrado.");
            return RedirectToAction("Index", "Friends");
        }

        var filterRequest = new Application.DTOs.Post.Requests.PostFilterRequest(
            null,
            null,
            null,
            null,
            null,
            1,
            20,
            null
        );
        var postsResult = await _postService.GetUserPostsAsync(
            _currentUserService.UserId!,
            id,
            filterRequest
        );
        var postVms = new List<PostListItemViewModel>();
        foreach (var d in postsResult.Items)
        {
            postVms.Add(
                new PostListItemViewModel
                {
                    Id = d.Id,
                    AuthorId = d.AuthorId,
                    AuthorName = d.AuthorName,
                    AuthorUserName = d.AuthorUserName,
                    AuthorProfilePicture = d.AuthorProfilePicture,
                    Content = d.Content,
                    ContentType = d.ContentType,
                    MediaPath =
                        d.ContentType == Domain.Enums.PostContentType.YouTubeVideo
                            ? WebApp.Helpers.YouTubeHelper.ToEmbedUrl(d.MediaPath)
                            : d.MediaPath,
                    Privacy = d.Privacy,
                    AllowComments = d.AllowComments,
                    IsEdited = d.IsEdited,
                    CreatedAt = d.CreatedAt.UtcDateTime,
                    LikesCount = d.LikesCount,
                    DislikesCount = d.DislikesCount,
                    CommentsCount = d.CommentsCount,
                    CurrentUserReaction = await _reactionService.GetUserReactionAsync(
                        _currentUserService.UserId!,
                        d.Id
                    ),
                    Comments = [],
                }
            );
        }

        var friendVm = new FriendListItemViewModel
        {
            FriendId = id,
            FriendName = $"{profileResult.FirstName} {profileResult.LastName}".Trim(),
            FriendUserName = profileResult.UserName,
            FriendProfilePicturePath = profileResult.ProfilePicturePath,
            CommonFriendsCount = 0,
        };

        var vm = new FriendDetailViewModel
        {
            Friend = friendVm,
            Posts = new PagedResultViewModel<PostListItemViewModel>
            {
                Items = postVms,
                Page = postsResult.Page,
                PageSize = postsResult.PageSize,
                TotalItems = postsResult.TotalCount,
            },
        };

        await this.PopulateMenuCountersAsync(
            _currentUserService,
            _friendRequestService,
            _notificationService
        );
        ViewBag.CurrentUserId = _currentUserService.UserId;
        return View("~/Views/Friends/Detail.cshtml", vm);
    }
}
