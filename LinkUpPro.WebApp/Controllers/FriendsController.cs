using LinkUpPro.Application.DTOs.Comment.Responses;
using LinkUpPro.Application.DTOs.Post.Requests;
using LinkUpPro.Application.DTOs.Post.Responses;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Application.ViewModels.CommentViewModels;
using LinkUpPro.Application.ViewModels.FriendshipViewModels;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Application.ViewModels.PostViewModels;
using LinkUpPro.Application.ViewModels.Shared;
using LinkUpPro.WebApp.Extensions;
using LinkUpPro.WebApp.Filters;
using LinkUpPro.WebApp.Helpers;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.WebApp.Controllers;

[SessionAuthorize]
public class FriendsController : BaseController
{
    private readonly IFriendshipService _friendshipService;
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly IReactionService _reactionService;
    private readonly IFriendRequestService _friendRequestService;
    private readonly INotificationService _notificationService;
    private readonly IProfileService _profileService;
    private readonly ILogger<FriendsController> _logger;

    public FriendsController(
        IFriendshipService friendshipService,
        IPostService postService,
        ICommentService commentService,
        IReactionService reactionService,
        IFriendRequestService friendRequestService,
        INotificationService notificationService,
        IProfileService profileService,
        ICurrentUserService currentUserService,
        ILogger<FriendsController> logger
    )
        : base(currentUserService)
    {
        _friendshipService = friendshipService;
        _postService = postService;
        _commentService = commentService;
        _reactionService = reactionService;
        _friendRequestService = friendRequestService;
        _notificationService = notificationService;
        _profileService = profileService;
        _logger = logger;
    }

    // ====================== INDEX ======================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _currentUserService.UserId!;

        var totalActive = await _friendshipService.GetActiveFriendsCountAsync(userId);
        var availablePosts = await _friendshipService.GetAvailablePostsCountAsync(userId);

        var vm = new FriendDetailViewModel
        {
            Summary = new FriendshipSummaryViewModel
            {
                TotalActiveFriends = totalActive,
                AvailablePostsCount = availablePosts
            }
        };

        await this.PopulateBaseViewModelAsync(vm, _currentUserService, _friendRequestService, _notificationService);
        ViewBag.CurrentUserId = userId;

        return View(vm);
    }

    // ====================== FEED POSTS (AJAX Partial) ======================

    [HttpGet]
    public async Task<IActionResult> FeedPosts(PostFilterViewModel filter, int page = 1)
    {
        var userId = _currentUserService.UserId!;

        var filterRequest = new PostFilterRequest(
            filter.SearchText,
            filter.ContentType,
            filter.FromDate,
            filter.ToDate,
            filter.EditedOnly,
            page,
            20,
            filter.FriendId,
            null
        );

        var postsResult = await _postService.GetFriendsPostsAsync(userId, filterRequest);
        var postViewModels = await MapToPostListItemsAsync(postsResult.Items);

        var vm = new FriendsFeedPartialViewModel
        {
            Filters = filter,
            Posts = new PagedResultViewModel<PostListItemViewModel>
            {
                Items = postViewModels,
                Page = postsResult.Page,
                PageSize = postsResult.PageSize,
                TotalItems = postsResult.TotalCount
            }
        };

        ViewBag.CurrentUserId = userId;
        ViewBag.AllFriends = await LoadAllFriendsForDropdownAsync(userId);

        return PartialView("_FeedPartial", vm);
    }

    [HttpGet]
    public async Task<IActionResult> FilterFeedPosts(PostFilterViewModel filter)
    {
        var userId = _currentUserService.UserId!;

        var filterRequest = new PostFilterRequest(
            filter.SearchText,
            filter.ContentType,
            filter.FromDate,
            filter.ToDate,
            filter.EditedOnly,
            1,
            20,
            filter.FriendId,
            null
        );

        var postsResult = await _postService.GetFriendsPostsAsync(userId, filterRequest);
        var postViewModels = await MapToPostListItemsAsync(postsResult.Items);
        ViewBag.CurrentUserAvatar = _currentUserService.ProfilePicturePath;

        return PartialView("~/Views/Posts/_PostList.cshtml", postViewModels);
    }

    [HttpGet]
    public async Task<IActionResult> LoadMoreFeedPosts(PostFilterViewModel filter, int page = 1)
    {
        var userId = _currentUserService.UserId!;

        var filterRequest = new PostFilterRequest(
            filter.SearchText,
            filter.ContentType,
            filter.FromDate,
            filter.ToDate,
            filter.EditedOnly,
            page,
            20,
            filter.FriendId,
            null
        );

        var postsResult = await _postService.GetFriendsPostsAsync(userId, filterRequest);
        if (postsResult.Items.Count == 0)
            return Content("");

        var postViewModels = await MapToPostListItemsAsync(postsResult.Items);
        ViewBag.CurrentUserAvatar = _currentUserService.ProfilePicturePath;

        return PartialView("~/Views/Posts/_PostList.cshtml", postViewModels);
    }

    // ====================== FRIENDS LIST (AJAX Partial) ======================

    [HttpGet]
    public async Task<IActionResult> FriendsListPartial(FriendSearchViewModel search, int page = 1)
    {
        var userId = _currentUserService.UserId!;

        var pagedResult = await _friendshipService.GetFriendsAsync(
            userId,
            search.SearchText,
            page,
            20
        );

        var friendViewModels = pagedResult.Items.Select(MapToFriendListItem).ToList();

        var vm = new FriendsListPartialViewModel
        {
            Search = search,
            Friends = friendViewModels,
            TotalCount = pagedResult.TotalCount,
            CurrentPage = page
        };

        ViewBag.CurrentUserId = userId;
        return PartialView("_FriendsListPartial", vm);
    }

    [HttpGet]
    public async Task<IActionResult> SearchFriends(FriendSearchViewModel search, int page = 1)
    {
        var userId = _currentUserService.UserId!;

        var pagedResult = await _friendshipService.GetFriendsAsync(
            userId,
            search.SearchText,
            page,
            20
        );

        var friendViewModels = pagedResult.Items.Select(MapToFriendListItem).ToList();

        var vm = new FriendsListPartialViewModel
        {
            Search = search,
            Friends = friendViewModels,
            TotalCount = pagedResult.TotalCount,
            CurrentPage = page
        };

        ViewBag.CurrentUserId = userId;
        return PartialView("_FriendsListInner", vm);
    }

    [HttpGet]
    public async Task<IActionResult> LoadMoreFriends(FriendSearchViewModel search, int page = 1)
    {
        var userId = _currentUserService.UserId!;

        var pagedResult = await _friendshipService.GetFriendsAsync(
            userId,
            search.SearchText,
            page,
            20
        );

        var friendViewModels = pagedResult.Items.Select(MapToFriendListItem).ToList();

        var vm = new FriendsListPartialViewModel
        {
            Search = search,
            Friends = friendViewModels,
            TotalCount = pagedResult.TotalCount,
            CurrentPage = page
        };

        return PartialView("_FriendsListInner", vm);
    }

    // ====================== DETAIL (Perfil de amigo) ======================

    [HttpGet]
    public async Task<IActionResult> Detail(string id, PostFilterViewModel filter)
    {
        var userId = _currentUserService.UserId!;

        var friendshipResult = await _friendshipService.GetFriendshipAsync(userId, id);
        if (!friendshipResult.IsSuccess)
        {
            ShowError("No posee permisos para visualizar el perfil de este usuario.");
            return RedirectToAction(nameof(Index));
        }

        var commonResult = await _friendshipService.GetCommonFriendsAsync(userId, id);
        var commonCount = commonResult.IsSuccess ? commonResult.Value!.Count : 0;

        var friendship = friendshipResult.Value!;
        var friendUser = await _profileService.GetByIdAsync(id);

        var friendVm = new FriendListItemViewModel
        {
            FriendId = id,
            FriendName = friendUser != null ? $"{friendUser.FirstName} {friendUser.LastName}".Trim() : string.Empty,
            FriendUserName = friendUser?.UserName ?? string.Empty,
            FriendProfilePicturePath = friendUser?.ProfilePicturePath,
            CommonFriendsCount = commonCount,
            FriendshipCreatedAt = friendship.CreatedAt.UtcDateTime
        };

        var filterRequest = filter.Adapt<PostFilterRequest>();
        var postsResult = await _postService.GetUserPostsAsync(userId, id, filterRequest);
        var postViewModels = await MapToPostListItemsAsync(postsResult.Items);

        var vm = new FriendDetailViewModel
        {
            Friend = friendVm,
            Posts = new PagedResultViewModel<PostListItemViewModel>
            {
                Items = postViewModels,
                Page = postsResult.Page,
                PageSize = postsResult.PageSize,
                TotalItems = postsResult.TotalCount
            },
            Filters = filter
        };

        await this.PopulateBaseViewModelAsync(vm, _currentUserService, _friendRequestService, _notificationService);
        ViewBag.CurrentUserId = userId;

        return View(vm);
    }

    // ====================== SEARCH (AJAX) ======================

    [HttpGet]
    public async Task<IActionResult> Search(string query)
    {
        var userId = _currentUserService.UserId!;
        var pagedResult = await _friendshipService.GetFriendsAsync(userId, query, 1, 50);
        var friends = pagedResult.Items.Select(MapToFriendListItem).ToList();
        return PartialView("_FriendCardList", friends);
    }

    // ====================== COMMON FRIENDS (Modal) ======================

    [HttpGet]
    public async Task<IActionResult> CommonFriends(string id)
    {
        var userId = _currentUserService.UserId!;
        var result = await _friendshipService.GetCommonFriendsAsync(userId, id);

        if (!result.IsSuccess)
        {
            return Json(new { success = false, message = result.Error?.Message });
        }

        var common = result.Value!;
        var vm = new CommonFriendsViewModel
        {
            User1Id = _currentUserService.UserId!,
            User2Id = id,
            Friends = common.Friends.Select(MapToFriendListItem).ToList()
        };

        return PartialView("_CommonFriendsModal", vm);
    }

    // ====================== REMOVE FRIEND ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(string friendId)
    {
        try
        {
            var result = await _friendshipService.DeleteAsync(_currentUserService.UserId!, friendId);
            if (!result.IsSuccess)
            {
                ShowError(result.Error?.Message ?? "No se pudo eliminar la amistad.");
            }
            else
            {
                ShowAlert("La amistad fue eliminada correctamente.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error eliminando amistad {FriendId}", friendId);
            ShowError("No se pudo eliminar la amistad.");
        }

        return RedirectToAction(nameof(Index));
    }

    // ====================== PRIVATE HELPERS ======================

    private async Task<List<object>> LoadAllFriendsForDropdownAsync(string userId)
    {
        var allFriends = await _friendshipService.GetFriendsAsync(userId, null, 1, 500);
        return allFriends.Items.Select(f => new
        {
            f.FriendId,
            Display = $"{f.FriendName} (@{f.FriendUserName})"
        } as object).ToList();
    }

    private FriendListItemViewModel MapToFriendListItem(LinkUpPro.Application.DTOs.Friendship.Responses.FriendListItemDto dto)
    {
        return new FriendListItemViewModel
        {
            FriendId = dto.FriendId,
            FriendName = dto.FriendName,
            FriendUserName = dto.FriendUserName,
            FriendProfilePicturePath = dto.FriendProfilePicturePath,
            CommonFriendsCount = dto.CommonFriendsCount
        };
    }

    private async Task<List<PostListItemViewModel>> MapToPostListItemsAsync(IEnumerable<PostListItemDto> items)
    {
        var list = items.ToList();
        if (list.Count == 0) return [];

        var userId = _currentUserService.UserId!;
        var postIds = list.Select(p => p.Id).ToList();
        var userReactions = await _reactionService.GetUserReactionsAsync(userId, postIds);

        var result = new List<PostListItemViewModel>(list.Count);
        foreach (var dto in list)
        {
            result.Add(await MapToPostListItemAsync(dto, userId, userReactions));
        }

        return result;
    }

    private async Task<PostListItemViewModel> MapToPostListItemAsync(
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
                ? YouTubeHelper.ToEmbedUrl(dto.MediaPath)
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

    private static CommentViewModel MapCommentTreeToViewModel(CommentTreeDto node, string currentUserId, bool canReply)
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
}
