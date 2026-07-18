using LinkUpPro.Application.DTOs.Comment.Requests;
using LinkUpPro.Application.DTOs.Comment.Responses;
using LinkUpPro.Application.DTOs.Profile.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.Interfaces.Repositories;
using Mapster;

namespace LinkUpPro.Application.Services;

public sealed class CommentService : ICommentService
{
    private const int DefaultRepliesPageSize = 5;
    private const int DefaultMaxThreadDepth = 5;

    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IProfileService _profileService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationRepository _notificationRepository;

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IFriendshipRepository friendshipRepository,
        IProfileService profileService,
        IUnitOfWork unitOfWork,
        INotificationRepository notificationRepository
    )
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _friendshipRepository = friendshipRepository;
        _profileService = profileService;
        _unitOfWork = unitOfWork;
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<CommentResponseDto>> CreateAsync(
        string authorId,
        CreateCommentRequest request
    )
    {
        var post = await _postRepository.GetByIdAsync(request.PostId);
        if (post is null)
            return Result<CommentResponseDto>.Failure(
                new DomainError("Post.NotFound", "La publicacion no fue encontrada.")
            );

        var isFriend =
            authorId == post.AuthorId
            || await _friendshipRepository.AreFriendsAsync(authorId, post.AuthorId);

        if (!post.CanComment(authorId, isFriend))
            return Result<CommentResponseDto>.Failure(
                new DomainError(
                    "Post.CannotComment",
                    "No tienes permiso para comentar en esta publicacion."
                )
            );

        var creationResult = Comment.Create(request.PostId, authorId, request.Content);
        if (creationResult.IsFailure)
            return Result<CommentResponseDto>.Failure(creationResult.Errors);

        await _commentRepository.AddAsync(creationResult.Value);
        await _unitOfWork.SaveChangesAsync();

        if (post.AuthorId != authorId)
        {
            var actor = await _profileService.GetByIdAsync(authorId);
            var actorName = actor is null ? "Alguien" : $"{actor.FirstName} {actor.LastName}".Trim();
            await CreateCommentNotificationAsync(post.AuthorId, authorId, request.PostId, actorName);
        }

        return await ToResponseDtoAsync(creationResult.Value);
    }

    public async Task<Result<CommentResponseDto>> CreateReplyAsync(
        string authorId,
        CreateReplyRequest request
    )
    {
        var parentComment = await _commentRepository.GetByIdAsync(request.ParentCommentId);
        if (parentComment is null)
            return Result<CommentResponseDto>.Failure(
                new DomainError("Comment.ParentNotFound", "El comentario padre no fue encontrado.")
            );

        if (parentComment.IsDeleted)
            return Result<CommentResponseDto>.Failure(
                new DomainError(
                    "Comment.ParentDeleted",
                    "No puedes responder a un comentario eliminado."
                )
            );

        var post = await _postRepository.GetByIdAsync(parentComment.PostId);
        if (post is null)
            return Result<CommentResponseDto>.Failure(
                new DomainError("Post.NotFound", "La publicacion no fue encontrada.")
            );

        var isFriend =
            authorId == post.AuthorId
            || await _friendshipRepository.AreFriendsAsync(authorId, post.AuthorId);

        if (!post.CanComment(authorId, isFriend))
            return Result<CommentResponseDto>.Failure(
                new DomainError(
                    "Post.CannotComment",
                    "No tienes permiso para comentar en esta publicacion."
                )
            );

        var creationResult = Comment.Create(
            parentComment.PostId,
            authorId,
            request.Content,
            request.ParentCommentId
        );
        if (creationResult.IsFailure)
            return Result<CommentResponseDto>.Failure(creationResult.Errors);

        await _commentRepository.AddAsync(creationResult.Value);
        await _unitOfWork.SaveChangesAsync();

        if (parentComment.AuthorId != authorId)
        {
            var actor = await _profileService.GetByIdAsync(authorId);
            var actorName = actor is null ? "Alguien" : $"{actor.FirstName} {actor.LastName}".Trim();
            await CreateReplyNotificationAsync(
                parentComment.AuthorId,
                authorId,
                parentComment.PostId,
                actorName
            );
        }

        return await ToResponseDtoAsync(creationResult.Value);
    }

    public async Task<Result<CommentResponseDto>> UpdateAsync(
        string authorId,
        long commentId,
        UpdateCommentRequest request
    )
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment is null)
            return Result<CommentResponseDto>.Failure(
                new DomainError("Comment.NotFound", "El comentario no fue encontrado.")
            );

        if (!comment.CanBeEditedBy(authorId))
            return Result<CommentResponseDto>.Failure(
                new DomainError(
                    "Comment.NotAuthorized",
                    "No tienes permiso para editar este comentario."
                )
            );

        var editResult = comment.Edit(request.Content);
        if (editResult.IsFailure)
            return Result<CommentResponseDto>.Failure(editResult.Errors);

        _commentRepository.Update(comment);
        await _unitOfWork.SaveChangesAsync();

        return await ToResponseDtoAsync(comment);
    }

    public async Task<Result> DeleteAsync(string authorId, long commentId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment is null)
            return Result.Failure(
                new DomainError("Comment.NotFound", "El comentario no fue encontrado.")
            );

        if (comment.AuthorId != authorId)
            return Result.Failure(
                new DomainError(
                    "Comment.NotAuthorized",
                    "No tienes permiso para eliminar este comentario."
                )
            );

        var hasReplies = await _commentRepository.HasRepliesAsync(commentId);
        comment.MarkAsDeleted(hasReplies);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<PagedResult<CommentTreeDto>> GetPostCommentsAsync(
        string requesterId,
        long postId,
        int page = 1,
        int pageSize = 10
    )
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post is null)
            return new PagedResult<CommentTreeDto>([], 0, page, pageSize);

        var isFriend =
            requesterId == post.AuthorId
            || await _friendshipRepository.AreFriendsAsync(requesterId, post.AuthorId);

        if (!post.CanBeViewedBy(requesterId, isFriend))
            return new PagedResult<CommentTreeDto>([], 0, page, pageSize);

        var rootOptions = new QueryOptions<Comment>
        {
            Skip = (page - 1) * pageSize,
            Take = pageSize,
            OrderBy = q => q.OrderBy(c => c.CreatedAt),
            IsTracking = false,
        };

        var rootComments = await _commentRepository.GetRootCommentsByPostAsync(postId, rootOptions);
        var totalRoots = await _commentRepository.CountRootCommentsByPostAsync(postId);

        if (rootComments.Count == 0)
            return new PagedResult<CommentTreeDto>([], totalRoots, page, pageSize);

        var userDict = await GetUsersDictionaryAsync(rootComments);

        var items = new List<CommentTreeDto>(rootComments.Count);
        foreach (var root in rootComments)
        {
            var node = await BuildNodeAsync(root, userDict, currentDepth: 0, maxDepth: DefaultMaxThreadDepth);
            items.Add(node);
        }

        return new PagedResult<CommentTreeDto>(items, totalRoots, page, pageSize);
    }

    public async Task<PagedResult<CommentTreeDto>> GetCommentRepliesAsync(
        string requesterId,
        long parentCommentId,
        int page = 1,
        int pageSize = 5,
        int currentDepth = 0
    )
    {
        var parent = await _commentRepository.GetByIdAsync(parentCommentId);
        if (parent is null)
            return new PagedResult<CommentTreeDto>([], 0, page, pageSize);

        var post = await _postRepository.GetByIdAsync(parent.PostId);
        if (post is null)
            return new PagedResult<CommentTreeDto>([], 0, page, pageSize);

        var isFriend =
            requesterId == post.AuthorId
            || await _friendshipRepository.AreFriendsAsync(requesterId, post.AuthorId);

        if (!post.CanBeViewedBy(requesterId, isFriend))
            return new PagedResult<CommentTreeDto>([], 0, page, pageSize);

        var replyOptions = new QueryOptions<Comment>
        {
            Skip = (page - 1) * pageSize,
            Take = pageSize,
            OrderBy = q => q.OrderBy(c => c.CreatedAt),
            IsTracking = false,
        };

        var replies = await _commentRepository.GetRepliesByParentAsync(
            parentCommentId,
            replyOptions
        );
        var totalReplies = await _commentRepository.CountRepliesByParentAsync(parentCommentId);

        if (replies.Count == 0)
            return new PagedResult<CommentTreeDto>([], totalReplies, page, pageSize);

        var userDict = await GetUsersDictionaryAsync(replies);

        var items = new List<CommentTreeDto>(replies.Count);
        foreach (var reply in replies)
        {
            var node = await BuildNodeAsync(reply, userDict, currentDepth: currentDepth, maxDepth: DefaultMaxThreadDepth);
            items.Add(node);
        }

        return new PagedResult<CommentTreeDto>(items, totalReplies, page, pageSize);
    }

    private async Task<Result<CommentResponseDto>> ToResponseDtoAsync(Comment comment)
    {
        var dto = comment.Adapt<CommentResponseDto>();

        var user = await _profileService.GetByIdAsync(comment.AuthorId);
        if (user is not null)
            dto = dto with
            {
                AuthorName = $"{user.FirstName} {user.LastName}".Trim(),
                AuthorProfilePicture = user.ProfilePicturePath,
            };

        var repliesCount = await _commentRepository.CountAsync(c =>
            c.ParentCommentId == comment.Id
        );
        dto = dto with { RepliesCount = repliesCount };

        return Result<CommentResponseDto>.Success(dto);
    }

    private async Task<Dictionary<string, UserResponseDto>> GetUsersDictionaryAsync(
        IReadOnlyCollection<Comment> comments
    )
    {
        var authorIds = comments.Select(c => c.AuthorId).Distinct().ToList();
        var users = await _profileService.GetByIdsAsync(authorIds);
        return users.ToDictionary(u => u.Key, u => u.Value);
    }

    private async Task<CommentTreeDto> BuildNodeAsync(
        Comment comment,
        Dictionary<string, UserResponseDto> userDict,
        int currentDepth,
        int maxDepth
    )
    {
        var dto = BuildCommentDto(comment, userDict);
        var replies = new List<CommentTreeDto>();
        var totalReplies = await _commentRepository.CountRepliesByParentAsync(comment.Id);
        var hasMore = totalReplies > 0;

        // Profundidad visual limitada a 5 niveles (0-4 visibles, 5+ truncados)
        var visualDepth = Math.Min(currentDepth, maxDepth - 1);
        var isTruncated = currentDepth >= maxDepth - 1;
        var showConnector = visualDepth < maxDepth - 1 && !isTruncated;

        string? replyingTo = null;
        if (isTruncated && comment.ParentCommentId.HasValue)
        {
            var parent = await _commentRepository.GetByIdAsync(comment.ParentCommentId.Value);
            if (parent is not null)
            {
                if (userDict.TryGetValue(parent.AuthorId, out var parentUser))
                    replyingTo = $"{parentUser.FirstName} {parentUser.LastName}".Trim();
                else
                {
                    // Fallback: load from DB if not in dictionary
                    var author = await _profileService.GetByIdAsync(parent.AuthorId);
                    if (author is not null)
                        replyingTo = $"{author.FirstName} {author.LastName}".Trim();
                }
            }
        }

        return new CommentTreeDto(
            dto,
            replies,
            TotalRepliesCount: totalReplies,
            HasMoreReplies: hasMore,
            CurrentRepliesPage: 1,
            RepliesPageSize: DefaultRepliesPageSize,
            VisualDepth: visualDepth,
            IsTruncated: isTruncated,
            ReplyingToUserName: replyingTo,
            ShowConnector: showConnector
        );
    }

    private static CommentResponseDto BuildCommentDto(
        Comment comment,
        Dictionary<string, UserResponseDto> userDict
    )
    {
        var dto = comment.Adapt<CommentResponseDto>();
        if (userDict.TryGetValue(comment.AuthorId, out var user))
            dto = dto with
            {
                AuthorName = $"{user.FirstName} {user.LastName}".Trim(),
                AuthorProfilePicture = user.ProfilePicturePath,
            };
        return dto;
    }

    private async Task CreateCommentNotificationAsync(
        string postAuthorId,
        string commentAuthorId,
        long postId,
        string actorName
    )
    {
        var notifResult = Notification.CreateComment(
            recipientId: postAuthorId,
            actorId: commentAuthorId,
            postId: postId,
            actorUserName: actorName
        );

        if (notifResult.IsSuccess)
            await _notificationRepository.AddAsync(notifResult.Value);
    }

    private async Task CreateReplyNotificationAsync(
        string parentAuthorId,
        string replyAuthorId,
        long postId,
        string actorName
    )
    {
        var notifResult = Notification.CreateReply(
            recipientId: parentAuthorId,
            actorId: replyAuthorId,
            postId: postId,
            actorUserName: actorName
        );

        if (notifResult.IsSuccess)
            await _notificationRepository.AddAsync(notifResult.Value);
    }
}
