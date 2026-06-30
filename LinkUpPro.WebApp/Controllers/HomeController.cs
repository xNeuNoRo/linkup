using LinkUpPro.Application.DTOs.Post.Requests;
using LinkUpPro.Application.DTOs.Post.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Application.ViewModels.CommentViewModels;
using LinkUpPro.Application.ViewModels.PostViewModels;
using LinkUpPro.Application.ViewModels.Shared;
using LinkUpPro.WebApp.Extensions;
using LinkUpPro.WebApp.Filters;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.WebApp.Controllers;

/// <summary>
/// Controlador del Home: feed propio del usuario autenticado,
/// crear, editar y eliminar publicaciones.
/// </summary>
[SessionAuthorize]
public class HomeController : BaseController
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly IReactionService _reactionService;
    private readonly IFriendRequestService _friendRequestService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IPostService postService,
        ICommentService commentService,
        IReactionService reactionService,
        IFriendRequestService friendRequestService,
        INotificationService notificationService,
        ICurrentUserService currentUserService,
        ILogger<HomeController> logger
    )
        : base(currentUserService)
    {
        _postService = postService;
        _commentService = commentService;
        _reactionService = reactionService;
        _friendRequestService = friendRequestService;
        _notificationService = notificationService;
        _logger = logger;
    }

    // ====================== INDEX (Feed propio) ======================

    [HttpGet]
    public async Task<IActionResult> Index(PostFilterViewModel filter)
    {
        var userId = _currentUserService.UserId!;

        // Aplicar preset si viene definido
        ApplyPreset(filter);

        // Mapear el ViewModel al DTO
        var filterRequest = filter.Adapt<PostFilterRequest>();

        var pagedResult = await _postService.GetMyPostsAsync(userId, filterRequest);

        // Mapear DTOs → ViewModels
        var postViewModels = await MapToPostListItemViewModelsAsync(pagedResult.Items);
        var pagedVm = new PagedResultViewModel<PostListItemViewModel>
        {
            Items = postViewModels,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalItems = pagedResult.TotalCount
        };

        var homeVm = new HomeViewModel
        {
            Filters = filter,
            Posts = pagedVm
        };

        await this.PopulateBaseViewModelAsync(homeVm, _currentUserService, _friendRequestService, _notificationService);

        // ViewBag con datos del usuario para los partials
        ViewBag.CurrentUserId = userId;
        ViewBag.CurrentUserAvatar = homeVm.CurrentUserProfilePicture;
        ViewBag.CurrentUserFullName = homeVm.CurrentUserFullName;

        return View(nameof(Index), homeVm);
    }

    // ====================== CREATE POST ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost(CreatePostViewModel model)
    {
        var userId = _currentUserService.UserId!;

        if (!ModelState.IsValid)
        {
            ShowError("Corrija los errores del formulario e intente nuevamente.");
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var request = model.Adapt<CreatePostRequest>();
            var result = await _postService.CreateAsync(userId, request);

            if (!result.IsSuccess)
            {
                ShowError(result.Error?.Message ?? "No se pudo crear la publicación.");
                return RedirectToAction(nameof(Index));
            }

            ShowAlert("La publicación fue creada correctamente.");
            return RedirectToAction(nameof(Index), new { page = 1 });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error creando publicación");
            ShowError("No se pudo crear la publicación. Inténtelo nuevamente.");
            return RedirectToAction(nameof(Index));
        }
    }

    // ====================== EDIT POST (GET) ======================

    [HttpGet]
    public async Task<IActionResult> EditPost(long id)
    {
        var userId = _currentUserService.UserId!;

        var result = await _postService.GetByIdAsync(userId, id);
        if (!result.IsSuccess)
        {
            ShowError("No se pudo cargar la publicación para editar.");
            return RedirectToAction(nameof(Index));
        }

        var post = result.Value;
        var youTubeUrl = post.ContentType == LinkUpPro.Domain.Enums.PostContentType.YouTubeVideo
            ? $"https://www.youtube.com/watch?v={post.MediaPath}"
            : null;
        var editVm = new UpdatePostViewModel
        {
            PostId = post.Id,
            Content = post.Content,
            ContentType = (int)post.ContentType,
            YouTubeUrl = youTubeUrl,
            CurrentImagePath = post.ContentType == LinkUpPro.Domain.Enums.PostContentType.Image ? post.MediaPath : null,
            CurrentYouTubeUrl = youTubeUrl,
            Privacy = (int)post.Privacy,
            AllowComments = post.AllowComments
        };

        return View(editVm);
    }

    // ====================== EDIT POST (POST) ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(UpdatePostViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var request = model.Adapt<UpdatePostRequest>();
            var result = await _postService.UpdateAsync(_currentUserService.UserId!, model.PostId, request);

            if (!result.IsSuccess)
            {
                ShowError(result.Error?.Message ?? "No se pudo actualizar la publicación.");
                return View(model);
            }

            ShowAlert("La publicación fue actualizada correctamente.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error editando publicación {PostId}", model.PostId);
            ShowError("No se pudo actualizar la publicación.");
            return View(model);
        }
    }

    // ====================== DELETE POST ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(long id)
    {
        try
        {
            var result = await _postService.DeleteAsync(_currentUserService.UserId!, id);
            if (!result.IsSuccess)
            {
                ShowError(result.Error?.Message ?? "No se pudo eliminar la publicación.");
            }
            else
            {
                ShowAlert("La publicación fue eliminada correctamente.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error eliminando publicación {PostId}", id);
            ShowError("No se pudo eliminar la publicación.");
        }

        return RedirectToAction(nameof(Index));
    }

    // ====================== LOAD MORE POSTS (AJAX Infinite Scroll) ======================

    [HttpGet]
    public async Task<IActionResult> LoadMorePosts(PostFilterViewModel filter)
    {
        var userId = _currentUserService.UserId!;
        ApplyPreset(filter);
        var filterRequest = filter.Adapt<PostFilterRequest>();
        var pagedResult = await _postService.GetMyPostsAsync(userId, filterRequest);
        if (pagedResult.Items.Count == 0)
            return Content("");

        var postViewModels = await MapToPostListItemViewModelsAsync(pagedResult.Items);
        ViewBag.CurrentUserAvatar = _currentUserService.ProfilePicturePath;
        return PartialView("~/Views/Posts/_PostList.cshtml", postViewModels);
    }

    // ====================== FILTER POSTS (AJAX) ======================

    [HttpGet]
    public async Task<IActionResult> FilterPosts(PostFilterViewModel filter)
    {
        var userId = _currentUserService.UserId!;
        ApplyPreset(filter);
        var filterRequest = filter.Adapt<PostFilterRequest>();
        var pagedResult = await _postService.GetMyPostsAsync(userId, filterRequest);

        var postViewModels = await MapToPostListItemViewModelsAsync(pagedResult.Items);
        ViewBag.CurrentUserAvatar = _currentUserService.ProfilePicturePath;
        return PartialView("~/Views/Posts/_PostList.cshtml", postViewModels);
    }

    // ====================== PRIVATE MAPPER ======================

    private async Task<List<PostListItemViewModel>> MapToPostListItemViewModelsAsync(IEnumerable<PostListItemDto> posts)
    {
        var items = posts.ToList();
        if (items.Count == 0) return [];

        var userId = _currentUserService.UserId!;
        var postIds = items.Select(p => p.Id).ToList();

        var userReactions = await _reactionService.GetUserReactionsAsync(userId, postIds);

        var result = new List<PostListItemViewModel>(items.Count);
        foreach (var dto in items)
        {
            result.Add(await MapToPostListItemViewModelAsync(dto, userId, userReactions));
        }

        return result;
    }

    private async Task<PostListItemViewModel> MapToPostListItemViewModelAsync(
        PostListItemDto dto,
        string userId,
        IReadOnlyDictionary<long, ReactionType?> userReactions
    )
    {
        var commentsResult = await _commentService.GetPostCommentsAsync(userId, dto.Id);
        var comments = commentsResult.Items.Any()
            ? commentsResult.Items.Select(c => MapCommentTreeToViewModel(c, userId, dto.AllowComments)).ToList()
            : [];

        return new PostListItemViewModel
        {
            Id = dto.Id,
            AuthorId = dto.AuthorId,
            AuthorName = dto.AuthorName,
            AuthorUserName = dto.AuthorUserName,
            AuthorProfilePicture = dto.AuthorProfilePicture,
            Content = dto.Content,
            ContentType = dto.ContentType,
            MediaPath = dto.ContentType == LinkUpPro.Domain.Enums.PostContentType.YouTubeVideo
                ? LinkUpPro.WebApp.Helpers.YouTubeHelper.ToEmbedUrl(dto.MediaPath)
                : dto.MediaPath,
            Privacy = dto.Privacy,
            AllowComments = dto.AllowComments,
            IsEdited = dto.IsEdited,
            CreatedAt = dto.CreatedAt.UtcDateTime,
            LikesCount = dto.LikesCount,
            DislikesCount = dto.DislikesCount,
            CommentsCount = dto.CommentsCount,
            CurrentUserReaction = userReactions.GetValueOrDefault(dto.Id),
            Comments = comments
        };
    }

    private static CommentViewModel MapCommentTreeToViewModel(
        LinkUpPro.Application.DTOs.Comment.Responses.CommentTreeDto node,
        string currentUserId,
        bool canReply
    )
    {
        var c = node.Comment;
        var vm = new CommentViewModel
        {
            Id = c.Id,
            PostId = c.PostId,
            ParentCommentId = c.ParentCommentId,
            AuthorId = c.AuthorId,
            AuthorName = c.AuthorName,
            AuthorProfilePicture = c.AuthorProfilePicture,
            Content = c.Content,
            IsEdited = c.IsEdited,
            CreatedAt = c.CreatedAt.UtcDateTime,
            UpdatedAt = c.UpdatedAt?.UtcDateTime,
            RepliesCount = node.TotalRepliesCount,
            HasMoreReplies = node.HasMoreReplies,
            VisualDepth = node.VisualDepth,
            IsTruncated = node.IsTruncated,
            ReplyingToUserName = node.ReplyingToUserName,
            ShowConnector = node.ShowConnector,
            Replies = node.Replies.Select(r => MapCommentTreeToViewModel(r, currentUserId, canReply && !c.IsDeleted)).ToList(),
            IsDeleted = c.IsDeleted,
            IsOwn = c.AuthorId == currentUserId,
            CanReply = canReply && !c.IsDeleted
        };
        return vm;
    }

    private static void ApplyPreset(PostFilterViewModel filter)
    {
        if (string.IsNullOrWhiteSpace(filter.Preset)) return;

        var now = DateTime.UtcNow;
        filter.FromDate = filter.Preset switch
        {
            "today" => now.Date,
            "week" => now.Date.AddDays(-(int)now.DayOfWeek),
            "month" => new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => filter.FromDate
        };
        filter.ToDate = filter.Preset switch
        {
            "today" => now.Date.AddDays(1).AddTicks(-1),
            "week" => now.Date.AddDays(7 - (int)now.DayOfWeek).AddTicks(-1),
            "month" => now.Date.AddMonths(1).AddDays(-(now.Day)).AddTicks(-1),
            _ => filter.ToDate
        };
        filter.Preset = null; // ya se aplicó
    }
}
