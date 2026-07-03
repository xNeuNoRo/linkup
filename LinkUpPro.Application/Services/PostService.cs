using FluentValidation;
using LinkUpPro.Application.DTOs.Post.Requests;
using LinkUpPro.Application.DTOs.Post.Responses;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.Exceptions;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.Interfaces.Repositories;
using LinkUpPro.Domain.ValueObjects;
using Mapster;

namespace LinkUpPro.Application.Services;

public sealed class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IReactionRepository _reactionRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IProfileService _profileService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;
    private readonly IValidator<CreatePostRequest> _createPostValidator;
    private readonly IValidator<PostFilterRequest> _filterValidator;

    public PostService(
        IPostRepository postRepository,
        IFriendshipRepository friendshipRepository,
        IReactionRepository reactionRepository,
        ICommentRepository commentRepository,
        IProfileService profileService,
        IUnitOfWork unitOfWork,
        IFileService fileService,
        IValidator<CreatePostRequest> createPostValidator,
        IValidator<PostFilterRequest> filterValidator
    )
    {
        _postRepository = postRepository;
        _friendshipRepository = friendshipRepository;
        _reactionRepository = reactionRepository;
        _commentRepository = commentRepository;
        _profileService = profileService;
        _unitOfWork = unitOfWork;
        _fileService = fileService;
        _createPostValidator = createPostValidator;
        _filterValidator = filterValidator;
    }

    public async Task<Result<PostResponseDto>> CreateAsync(
        string authorId,
        CreatePostRequest request
    )
    {
        var validationResult = await _createPostValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var error = validationResult.Errors.First();
            return Result<PostResponseDto>.Failure(
                new DomainError("Post.Validation." + error.PropertyName, error.ErrorMessage)
            );
        }

        var contentType = (PostContentType)request.ContentType;
        var privacy = (PrivacyLevel)request.Privacy;

        // Validación defensiva: no permitir imagen y YouTube simultáneamente
        if (request.ImageFile is not null && !string.IsNullOrEmpty(request.YouTubeUrl))
            return Result<PostResponseDto>.Failure(
                new DomainError(
                    "Post.MediaConflict",
                    "No debe permitirse enviar simultáneamente una imagen y un enlace de YouTube."
                )
            );

        string mediaPath;
        try
        {
            mediaPath = contentType switch
            {
                PostContentType.YouTubeVideo => YouTubeVideoId
                    .Create(request.YouTubeUrl ?? string.Empty)
                    .Value,
                PostContentType.Image => await ValidateAndUploadImageAsync(request.ImageFile),
                _ => throw new DomainException(
                    "Post.InvalidContentType",
                    "El tipo de contenido seleccionado no es valido."
                ),
            };
        }
        catch (DomainException ex)
        {
            return Result<PostResponseDto>.Failure(new DomainError(ex.Code, ex.Message));
        }

        var postResult = Post.Create(
            authorId,
            request.Content,
            contentType,
            mediaPath,
            privacy,
            request.AllowComments
        );
        if (postResult.IsFailure)
            return Result<PostResponseDto>.Failure(postResult.Errors);

        await _postRepository.AddAsync(postResult.Value);
        await _unitOfWork.SaveChangesAsync();

        return await ToResponseDtoAsync(postResult.Value);
    }

    public async Task<Result<PostResponseDto>> GetByIdAsync(string requesterId, long postId)
    {
        var post = await _postRepository.GetByIdWithDetailsAsync(postId);
        if (post is null)
            return Result<PostResponseDto>.Failure(
                new DomainError("Post.NotFound", "Publicacion no encontrada.")
            );

        var isFriend = await _friendshipRepository.AreFriendsAsync(requesterId, post.AuthorId);

        if (!post.CanBeViewedBy(requesterId, isFriend))
            return Result<PostResponseDto>.Failure(
                new DomainError("Post.NotFound", "Publicacion no encontrada.")
            );

        return await ToResponseDtoAsync(post);
    }

    public async Task<Result<PostResponseDto>> UpdateAsync(
        string authorId,
        long postId,
        UpdatePostRequest request
    )
    {
        var post = await _postRepository.GetByIdWithDetailsAsync(postId);
        if (post is null)
            return Result<PostResponseDto>.Failure(
                new DomainError("Post.NotFound", "Publicacion no encontrada.")
            );

        if (!post.CanBeEditedBy(authorId))
            return Result<PostResponseDto>.Failure(
                new DomainError(
                    "Post.NotAuthorized",
                    "No tienes permiso para editar esta publicacion."
                )
            );

        // Validación defensiva: no permitir imagen y YouTube simultáneamente
        if (request.ImageFile is not null && request.ImageFile.Length > 0 && !string.IsNullOrEmpty(request.YouTubeUrl))
            return Result<PostResponseDto>.Failure(
                new DomainError(
                    "Post.MediaConflict",
                    "No debe permitirse enviar simultáneamente una imagen y un enlace de YouTube."
                )
            );

        var content = request.Content ?? post.Content;
        var contentType = request.ContentType.HasValue
            ? (PostContentType)request.ContentType.Value
            : post.ContentType;

        var isContentTypeChanged = request.ContentType.HasValue && (int)post.ContentType != request.ContentType.Value;
        var oldMediaPath = post.MediaPath;
        var oldContentType = post.ContentType;

        string mediaPath;
        try
        {
            if (contentType == PostContentType.YouTubeVideo)
            {
                if (!string.IsNullOrWhiteSpace(request.YouTubeUrl))
                {
                    mediaPath = YouTubeVideoId.Create(request.YouTubeUrl).Value;
                }
                else if (isContentTypeChanged)
                {
                    return Result<PostResponseDto>.Failure(
                        new DomainError("Post.YouTubeUrlRequired", "Debe ingresar un enlace valido de YouTube.")
                    );
                }
                else
                {
                    // Mismo tipo YouTube, conservar URL actual
                    mediaPath = post.MediaPath;
                }
            }
            else if (contentType == PostContentType.Image)
            {
                if (request.ImageFile is not null && request.ImageFile.Length > 0)
                {
                    mediaPath = await ValidateAndUploadImageAsync(request.ImageFile);
                }
                else if (isContentTypeChanged)
                {
                    return Result<PostResponseDto>.Failure(
                        new DomainError("Post.ImageRequired", "Debe seleccionar una imagen para la publicacion.")
                    );
                }
                else
                {
                    // Mismo tipo Image, conservar imagen actual
                    mediaPath = post.MediaPath;
                }
            }
            else
            {
                return Result<PostResponseDto>.Failure(
                    new DomainError("Post.InvalidContentType", "El tipo de contenido seleccionado no es valido.")
                );
            }

            // Limpiar media anterior cuando corresponde
            var hasNewFile = request.ImageFile is not null && request.ImageFile.Length > 0;
            var hasNewYouTubeUrl = !string.IsNullOrWhiteSpace(request.YouTubeUrl);

            if (isContentTypeChanged && oldContentType == PostContentType.Image && !string.IsNullOrEmpty(oldMediaPath))
            {
                // Cambio de Image → YouTube: eliminar imagen anterior del disco
                await _fileService.DeleteFileAsync(oldMediaPath);
            }
            else if (!isContentTypeChanged && contentType == PostContentType.Image && hasNewFile && !string.IsNullOrEmpty(oldMediaPath))
            {
                // Mismo Image pero nueva imagen: eliminar anterior del disco
                await _fileService.DeleteFileAsync(oldMediaPath);
            }
        }
        catch (DomainException ex)
        {
            return Result<PostResponseDto>.Failure(new DomainError(ex.Code, ex.Message));
        }

        var privacy = request.Privacy.HasValue ? (PrivacyLevel)request.Privacy.Value : post.Privacy;
        var allowComments = request.AllowComments ?? post.AllowComments;

        var editResult = post.Edit(content, contentType, mediaPath, privacy, allowComments);
        if (editResult.IsFailure)
            return Result<PostResponseDto>.Failure(editResult.Errors);

        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync();

        return await ToResponseDtoAsync(post);
    }

    public async Task<Result> DeleteAsync(string authorId, long postId)
    {
        var post = await _postRepository.GetByIdWithDetailsAsync(postId);
        if (post is null)
            return Result.Failure(new DomainError("Post.NotFound", "Publicacion no encontrada."));

        if (!post.CanBeEditedBy(authorId))
            return Result.Failure(
                new DomainError(
                    "Post.NotAuthorized",
                    "No tienes permiso para eliminar esta publicacion."
                )
            );

        post.MarkAsDeleted();
        _postRepository.Update(post);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<PagedResult<PostListItemDto>> GetMyPostsAsync(
        string userId,
        PostFilterRequest filter
    )
    {
        var validationResult = await _filterValidator.ValidateAsync(filter);
        if (!validationResult.IsValid)
        {
            var clamped = filter with
            {
                Page = Math.Max(1, filter.Page),
                PageSize = Math.Clamp(filter.PageSize, 1, 100),
            };
            filter = clamped;
        }

        var contentType = filter.ContentType.HasValue
            ? (PostContentType?)filter.ContentType.Value
            : null;

        var options = new QueryOptions<Post>
        {
            Skip = Math.Max(0, (filter.Page - 1) * filter.PageSize),
            Take = Math.Max(1, filter.PageSize),
            OrderBy = q => q.OrderByDescending(p => p.CreatedAt),
            IsTracking = false,
        };

        var paged = await _postRepository.SearchAuthorPostsAsync(
            userId,
            filter.SearchText,
            contentType,
            filter.FromDate,
            filter.ToDate,
            filter.EditedOnly,
            options
        );

        var total = await _postRepository.CountAsync(p =>
            p.AuthorId == userId
            && (contentType == null || p.ContentType == contentType)
            && (
                string.IsNullOrWhiteSpace(filter.SearchText)
                || p.Content.Contains(filter.SearchText)
            )
            && (!filter.FromDate.HasValue || p.CreatedAt >= filter.FromDate)
            && (!filter.ToDate.HasValue || p.CreatedAt <= filter.ToDate)
            && (filter.EditedOnly != true || p.IsEdited)
        );

        var items = await MapToListItemDtosAsync(paged);

        return new PagedResult<PostListItemDto>(items, total, filter.Page, filter.PageSize);
    }

    public async Task<PagedResult<PostListItemDto>> GetFriendsPostsAsync(
        string userId,
        PostFilterRequest filter
    )
    {
        var contentType = filter.ContentType.HasValue
            ? (PostContentType?)filter.ContentType.Value
            : null;

        var options = new QueryOptions<Post>
        {
            Skip = Math.Max(0, (filter.Page - 1) * filter.PageSize),
            Take = Math.Max(1, filter.PageSize),
            OrderBy = q => q.OrderByDescending(p => p.CreatedAt),
            IsTracking = false,
        };

        var paged = await _postRepository.SearchFriendsPostsAsync(
            userId,
            filter.SearchText,
            filter.FriendId,
            contentType,
            filter.FromDate,
            filter.ToDate,
            filter.EditedOnly,
            options
        );

        var total = await _postRepository.CountAsync(p =>
            (p.AuthorId != userId)
            && (p.Privacy == PrivacyLevel.FriendsOnly)
            && (contentType == null || p.ContentType == contentType)
            && (
                string.IsNullOrWhiteSpace(filter.SearchText)
                || p.Content.Contains(filter.SearchText)
            )
            && (!filter.FromDate.HasValue || p.CreatedAt >= filter.FromDate)
            && (!filter.ToDate.HasValue || p.CreatedAt <= filter.ToDate)
            && (filter.EditedOnly != true || p.IsEdited)
        );

        var items = await MapToListItemDtosAsync(paged);

        return new PagedResult<PostListItemDto>(items, total, filter.Page, filter.PageSize);
    }

    public async Task<PagedResult<PostListItemDto>> GetUserPostsAsync(
        string requesterId,
        string targetUserId,
        PostFilterRequest filter
    )
    {
        var contentType = filter.ContentType.HasValue
            ? (PostContentType?)filter.ContentType.Value
            : null;

        var isFriend = await _friendshipRepository.AreFriendsAsync(requesterId, targetUserId);

        var options = new QueryOptions<Post>
        {
            Skip = Math.Max(0, (filter.Page - 1) * filter.PageSize),
            Take = Math.Max(1, filter.PageSize),
            OrderBy = q => q.OrderByDescending(p => p.CreatedAt),
            IsTracking = false,
        };

        var posts = await _postRepository.SearchAuthorPostsAsync(
            targetUserId,
            filter.SearchText,
            contentType,
            filter.FromDate,
            filter.ToDate,
            filter.EditedOnly,
            options
        );

        var visiblePosts = posts.Where(p => p.CanBeViewedBy(requesterId, isFriend)).ToList();

        var isSelf = requesterId == targetUserId;
        var total = await _postRepository.CountAsync(p =>
            p.AuthorId == targetUserId
            && (isSelf || (p.Privacy == PrivacyLevel.FriendsOnly && isFriend))
        );

        var items = await MapToListItemDtosAsync(visiblePosts);

        return new PagedResult<PostListItemDto>(items, total, filter.Page, filter.PageSize);
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

    private async Task<Result<PostResponseDto>> ToResponseDtoAsync(Post post)
    {
        var dto = post.Adapt<PostResponseDto>();
        var user = await _profileService.GetByIdAsync(post.AuthorId);

        if (user is not null)
            dto = dto with
            {
                AuthorName = $"{user.FirstName} {user.LastName}".Trim(),
                AuthorProfilePicture = user.ProfilePicturePath,
            };

        return Result<PostResponseDto>.Success(dto);
    }

    private async Task<string> ValidateAndUploadImageAsync(
        Microsoft.AspNetCore.Http.IFormFile? imageFile
    )
    {
        if (imageFile is null || imageFile.Length == 0)
            throw new DomainException(
                "Debe seleccionar una imagen para crear la publicacion.",
                "Post.ImageRequired"
            );

        if (!_fileService.IsImageValid(imageFile))
            throw new DomainException(
                "El archivo seleccionado no tiene un formato de imagen valido o supera los 5 MB.",
                "Post.InvalidImage"
            );

        return await _fileService.UploadFileAsync(imageFile, "posts");
    }

    private async Task<IReadOnlyCollection<PostListItemDto>> MapToListItemDtosAsync(
        IReadOnlyCollection<Post> posts
    )
    {
        if (posts.Count == 0)
            return Array.Empty<PostListItemDto>();

        var dtos = new List<PostListItemDto>(posts.Count);

        var authorIds = posts.Select(p => p.AuthorId).Distinct().ToList();
        var userDict = await _profileService.GetByIdsAsync(authorIds);

        var postIds = posts.Select(p => p.Id).ToList();
        var reactionCounts = await _reactionRepository.GetCountsForPostsAsync(postIds);
        var commentCounts = await _commentRepository.GetCountsForPostsAsync(postIds);

        foreach (var post in posts)
        {
            var dto = post.Adapt<PostListItemDto>();

            if (userDict.TryGetValue(post.AuthorId, out var user))
                dto = dto with
                {
                    AuthorName = $"{user.FirstName} {user.LastName}".Trim(),
                    AuthorUserName = user.UserName,
                    AuthorProfilePicture = user.ProfilePicturePath,
                };

            var counts = reactionCounts.GetValueOrDefault(post.Id, new ReactionCounts(0, 0));
            var commentsCount = commentCounts.GetValueOrDefault(post.Id, 0);

            dtos.Add(
                dto with
                {
                    LikesCount = counts.Likes,
                    DislikesCount = counts.Dislikes,
                    CommentsCount = commentsCount,
                }
            );
        }

        return dtos;
    }
}
