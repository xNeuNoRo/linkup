using LinkUpPro.Application.DTOs.Post.Requests;
using LinkUpPro.Application.DTOs.Post.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.Interfaces.Repositories;
using Mapster;

namespace LinkUpPro.Application.Services;

public sealed class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IReactionRepository _reactionRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PostService(
        IPostRepository postRepository,
        IFriendshipRepository friendshipRepository,
        IReactionRepository reactionRepository,
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWork)
    {
        _postRepository = postRepository;
        _friendshipRepository = friendshipRepository;
        _reactionRepository = reactionRepository;
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PostResponseDto>> CreateAsync(string authorId, CreatePostRequest request)
    {
        var mediaPath = (request.MediaPath ?? request.YouTubeUrl ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(mediaPath))
        {
            return Result<PostResponseDto>.Failure(
                new DomainError("Post.MediaRequired", "Debe seleccionar una imagen o ingresar un enlace valido de YouTube."));
        }

        var contentType = (PostContentType)request.ContentType;
        var privacy = (PrivacyLevel)request.Privacy;

        var postResult = Post.Create(authorId, request.Content, contentType, mediaPath, privacy, request.AllowComments);

        if (postResult.IsFailure)
        {
            return Result<PostResponseDto>.Failure(postResult.Errors);
        }

        await _postRepository.AddAsync(postResult.Value);
        await _unitOfWork.SaveChangesAsync();

        var dto = postResult.Value.Adapt<PostResponseDto>();

        return Result<PostResponseDto>.Success(dto);
    }

    public async Task<Result<PostResponseDto>> GetByIdAsync(string requesterId, long postId)
    {
        var post = await _postRepository.GetByIdWithDetailsAsync(postId);

        if (post is null)
        {
            return Result<PostResponseDto>.Failure(
                new DomainError("Post.NotFound", "Publicacion no encontrada."));
        }

        var isFriend = await _friendshipRepository.AreFriendsAsync(requesterId, post.AuthorId);

        if (!post.CanBeViewedBy(requesterId, isFriend))
        {
            return Result<PostResponseDto>.Failure(
                new DomainError("Post.NotFound", "Publicacion no encontrada."));
        }

        var dto = post.Adapt<PostResponseDto>();

        return Result<PostResponseDto>.Success(dto);
    }

    public async Task<Result<PostResponseDto>> UpdateAsync(string authorId, long postId, UpdatePostRequest request)
    {
        var post = await _postRepository.GetByIdWithDetailsAsync(postId);

        if (post is null)
        {
            return Result<PostResponseDto>.Failure(
                new DomainError("Post.NotFound", "Publicacion no encontrada."));
        }

        if (!post.CanBeEditedBy(authorId))
        {
            return Result<PostResponseDto>.Failure(
                new DomainError("Post.NotAuthorized", "No tienes permiso para editar esta publicacion."));
        }

        var content = request.Content ?? post.Content;
        var contentType = request.ContentType.HasValue
            ? (PostContentType)request.ContentType.Value
            : post.ContentType;
        var mediaPath = (request.MediaPath ?? request.YouTubeUrl ?? post.MediaPath).Trim();
        var privacy = request.Privacy.HasValue
            ? (PrivacyLevel)request.Privacy.Value
            : post.Privacy;
        var allowComments = request.AllowComments ?? post.AllowComments;

        var editResult = post.Edit(content, contentType, mediaPath, privacy, allowComments);

        if (editResult.IsFailure)
        {
            return Result<PostResponseDto>.Failure(editResult.Errors);
        }

        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync();

        var dto = post.Adapt<PostResponseDto>();

        return Result<PostResponseDto>.Success(dto);
    }

    public async Task<Result> DeleteAsync(string authorId, long postId)
    {
        var post = await _postRepository.GetByIdWithDetailsAsync(postId);

        if (post is null)
        {
            return Result.Failure(
                new DomainError("Post.NotFound", "Publicacion no encontrada."));
        }

        if (!post.CanBeEditedBy(authorId))
        {
            return Result.Failure(
                new DomainError("Post.NotAuthorized", "No tienes permiso para eliminar esta publicacion."));
        }

        post.MarkAsDeleted();
        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<PagedResult<PostListItemDto>> GetMyPostsAsync(string userId, PostFilterRequest filter)
    {
        PostContentType? contentType = filter.ContentType.HasValue
            ? (PostContentType)filter.ContentType.Value
            : null;

        var options = new QueryOptions<Post>
        {
            Skip = (filter.Page - 1) * filter.PageSize,
            Take = filter.PageSize,
            OrderBy = q => q.OrderByDescending(p => p.CreatedAt),
            IsTracking = false
        };

        var posts = await _postRepository.SearchAuthorPostsAsync(
            userId, filter.SearchText, contentType,
            filter.FromDate, filter.ToDate, filter.EditedOnly, options);

        var total = await _postRepository.CountAsync(p => p.AuthorId == userId);

        var items = await MapToListItemDtosAsync(posts);

        return new PagedResult<PostListItemDto>(items, total, filter.Page, filter.PageSize);
    }

    public async Task<PagedResult<PostListItemDto>> GetFriendsPostsAsync(string userId, PostFilterRequest filter)
    {
        PostContentType? contentType = filter.ContentType.HasValue
            ? (PostContentType)filter.ContentType.Value
            : null;

        var options = new QueryOptions<Post>
        {
            Skip = (filter.Page - 1) * filter.PageSize,
            Take = filter.PageSize,
            OrderBy = q => q.OrderByDescending(p => p.CreatedAt),
            IsTracking = false
        };

        var posts = await _postRepository.SearchFriendsPostsAsync(
            userId, filter.SearchText, null, contentType,
            filter.FromDate, filter.ToDate, filter.EditedOnly, options);

        var items = await MapToListItemDtosAsync(posts);

        return new PagedResult<PostListItemDto>(items, items.Count, filter.Page, filter.PageSize);
    }

    public async Task<PagedResult<PostListItemDto>> GetUserPostsAsync(
        string requesterId, string targetUserId, PostFilterRequest filter)
    {
        PostContentType? contentType = filter.ContentType.HasValue
            ? (PostContentType)filter.ContentType.Value
            : null;

        var isFriend = await _friendshipRepository.AreFriendsAsync(requesterId, targetUserId);

        var posts = await _postRepository.SearchAuthorPostsAsync(
            targetUserId, filter.SearchText, contentType,
            filter.FromDate, filter.ToDate, filter.EditedOnly);

        var visiblePosts = posts
            .Where(p => p.CanBeViewedBy(requesterId, isFriend))
            .OrderByDescending(p => p.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        var items = await MapToListItemDtosAsync(visiblePosts);

        return new PagedResult<PostListItemDto>(items, items.Count, filter.Page, filter.PageSize);
    }

    public async Task<Result<PostStatsDto>> GetStatsAsync(string userId)
    {
        var posts = await _postRepository.GetByAuthorAsync(userId);

        var stats = new PostStatsDto(
            TotalPosts: posts.Count,
            ImagePosts: posts.Count(p => p.ContentType == PostContentType.Image),
            VideoPosts: posts.Count(p => p.ContentType == PostContentType.YouTubeVideo),
            FriendsOnlyPosts: posts.Count(p => p.Privacy == PrivacyLevel.FriendsOnly),
            OnlyMePosts: posts.Count(p => p.Privacy == PrivacyLevel.OnlyMe),
            EditedPosts: posts.Count(p => p.IsEdited)
        );

        return Result<PostStatsDto>.Success(stats);
    }

    private async Task<IReadOnlyCollection<PostListItemDto>> MapToListItemDtosAsync(
        IReadOnlyCollection<Post> posts)
    {
        var dtos = new List<PostListItemDto>(posts.Count);

        foreach (var post in posts)
        {
            var dto = post.Adapt<PostListItemDto>();
            var counts = await _reactionRepository.GetCountsByPostAsync(post.Id);
            var commentsCount = await _commentRepository.CountByPostAsync(post.Id);

            dtos.Add(dto with
            {
                LikesCount = counts.Likes,
                DislikesCount = counts.Dislikes,
                CommentsCount = commentsCount
            });
        }

        return dtos;
    }
}
