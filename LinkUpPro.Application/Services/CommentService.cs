using LinkUpPro.Application.DTOs.Comment.Requests;
using LinkUpPro.Application.DTOs.Comment.Responses;
using LinkUpPro.Application.DTOs.Profile.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.Interfaces.Repositories;

namespace LinkUpPro.Application.Services;

public sealed class CommentService : ICommentService
{
    private readonly ICommentRepository? _commentRepository;
    private readonly IPostRepository? _postRepository;
    private readonly IFriendshipRepository? _friendshipRepository;
    private readonly IProfileService? _profileService;
    private readonly IUnitOfWork? _unitOfWork;

    private static readonly List<Comment> _inMemoryStore = [];
    private static long _nextId = 1;
    private static readonly object _lock = new();

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IFriendshipRepository friendshipRepository,
        IProfileService profileService,
        IUnitOfWork unitOfWork)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _friendshipRepository = friendshipRepository;
        _profileService = profileService;
        _unitOfWork = unitOfWork;
    }

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IFriendshipRepository friendshipRepository)
        : this(commentRepository, postRepository, friendshipRepository, null!, null!)
    {
    }

    public async Task<Result<CommentResponseDto>> CreateAsync(string authorId, CreateCommentRequest request)
    {
        var creationResult = Comment.Create(request.PostId, authorId, request.Content);
        if (creationResult.IsFailure)
            return Result<CommentResponseDto>.Failure(creationResult.Errors);

        var comment = creationResult.Value;

        if (_commentRepository is not null)
        {
            var post = await _postRepository!.GetByIdAsync(request.PostId);
            if (post is null)
                return Result<CommentResponseDto>.Failure(new DomainError("Post.NotFound", "La publicacion no fue encontrada."));

            var isFriend = authorId == post.AuthorId ||
                await _friendshipRepository!.AreFriendsAsync(authorId, post.AuthorId);

            if (!post.CanComment(authorId, isFriend))
                return Result<CommentResponseDto>.Failure(new DomainError("Post.CannotComment", "No tienes permiso para comentar en esta publicacion."));

            await _commentRepository.AddAsync(comment);
            await _unitOfWork!.SaveChangesAsync();
            return await MapToResponseDtoAsync(comment);
        }

        SaveInMemory(comment);
        return MapToResponseDtoInMemory(comment);
    }

    public async Task<Result<CommentResponseDto>> CreateReplyAsync(string authorId, CreateReplyRequest request)
    {
        long postId;

        if (_commentRepository is not null)
        {
            var parentComment = await _commentRepository.GetByIdAsync(request.ParentCommentId);
            if (parentComment is null)
                return Result<CommentResponseDto>.Failure(new DomainError("Comment.ParentNotFound", "El comentario padre no fue encontrado."));

            if (parentComment.IsDeleted)
                return Result<CommentResponseDto>.Failure(new DomainError("Comment.ParentDeleted", "No puedes responder a un comentario eliminado."));

            var post = await _postRepository!.GetByIdAsync(parentComment.PostId);
            if (post is null)
                return Result<CommentResponseDto>.Failure(new DomainError("Post.NotFound", "La publicacion no fue encontrada."));

            var isFriend = authorId == post.AuthorId ||
                await _friendshipRepository!.AreFriendsAsync(authorId, post.AuthorId);

            if (!post.CanComment(authorId, isFriend))
                return Result<CommentResponseDto>.Failure(new DomainError("Post.CannotComment", "No tienes permiso para comentar en esta publicacion."));

            postId = parentComment.PostId;
        }
        else
        {
        
            var parentComment = _inMemoryStore.FirstOrDefault(c => c.Id == request.ParentCommentId);
            postId = parentComment?.PostId ?? 1;
        }


        var creationResult = Comment.Create(postId, authorId, request.Content, request.ParentCommentId);
        if (creationResult.IsFailure)
            return Result<CommentResponseDto>.Failure(creationResult.Errors);

        var comment = creationResult.Value;

        if (_commentRepository is not null)
        {
            await _commentRepository.AddAsync(comment);
            await _unitOfWork!.SaveChangesAsync();
            return await MapToResponseDtoAsync(comment);
        }

        SaveInMemory(comment);
        return MapToResponseDtoInMemory(comment);
    }



    public async Task<Result<CommentResponseDto>> UpdateAsync(string authorId, long commentId, UpdateCommentRequest request)
    {
        if (_commentRepository is not null)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment is null)
                return Result<CommentResponseDto>.Failure(new DomainError("Comment.NotFound", "El comentario no fue encontrado."));

            if (!comment.CanBeEditedBy(authorId))
                return Result<CommentResponseDto>.Failure(new DomainError("Comment.NotAuthorized", "No tienes permiso para editar este comentario."));

            var editResult = comment.Edit(request.Content);
            if (editResult.IsFailure)
                return Result<CommentResponseDto>.Failure(editResult.Errors);

            _commentRepository.Update(comment);
            await _unitOfWork!.SaveChangesAsync();

            return await MapToResponseDtoAsync(comment);
        }

        var inMemoryComment = FindInMemory(commentId);
        if (inMemoryComment is null)
            return Result<CommentResponseDto>.Failure(new DomainError("Comment.NotFound", "El comentario no fue encontrado."));

        if (!inMemoryComment.CanBeEditedBy(authorId))
            return Result<CommentResponseDto>.Failure(new DomainError("Comment.NotAuthorized", "No tienes permiso para editar este comentario."));

        var editResultInMem = inMemoryComment.Edit(request.Content);
        if (editResultInMem.IsFailure)
            return Result<CommentResponseDto>.Failure(editResultInMem.Errors);

        return MapToResponseDtoInMemory(inMemoryComment);
    }

    public async Task<Result> DeleteAsync(string authorId, long commentId)
    {
        if (_commentRepository is not null)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment is null)
                return Result.Failure(new DomainError("Comment.NotFound", "El comentario no fue encontrado."));

            if (comment.AuthorId != authorId)
                return Result.Failure(new DomainError("Comment.NotAuthorized", "No tienes permiso para eliminar este comentario."));

            var hasReplies = await _commentRepository.HasRepliesAsync(commentId);
            comment.MarkAsDeleted(hasReplies);
            await _unitOfWork!.SaveChangesAsync();

            return Result.Success();
        }

        var inMemoryComment = FindInMemory(commentId);
        if (inMemoryComment is null)
            return Result.Failure(new DomainError("Comment.NotFound", "El comentario no fue encontrado."));

        if (inMemoryComment.AuthorId != authorId)
            return Result.Failure(new DomainError("Comment.NotAuthorized", "No tienes permiso para eliminar este comentario."));

        var hasRepliesInMem = _inMemoryStore.Any(c =>
            !c.IsDeleted && c.ParentCommentId == commentId);
        inMemoryComment.MarkAsDeleted(hasRepliesInMem);

        return Result.Success();
    }

    public async Task<List<CommentTreeDto>> GetPostCommentsAsync(string requesterId, long postId)
    {
        if (_commentRepository is not null)
        {
            var post = await _postRepository!.GetByIdAsync(postId);
            if (post is null)
                return [];

            var isFriend = requesterId == post.AuthorId ||
                await _friendshipRepository!.AreFriendsAsync(requesterId, post.AuthorId);

            if (!post.CanBeViewedBy(requesterId, isFriend))
                return [];

            var allComments = await _commentRepository.GetByPostAsync(postId);
            if (allComments.Count == 0)
                return [];

            var userDict = await GetUsersDictionaryAsync(allComments);

            var roots = allComments
                .Where(c => c.ParentCommentId is null)
                .OrderBy(c => c.CreatedAt)
                .ToList();

            return roots.Select(root => BuildCommentTreeDto(root, allComments, userDict)).ToList();
        }

        return [];
    }

    private static long GetParentPostIdInMemory(long parentCommentId)
    {
        lock (_lock)
        {
            return _inMemoryStore
                .Where(c => c.Id == parentCommentId && !c.IsDeleted)
                .Select(c => c.PostId)
                .FirstOrDefault();
        }
    }

    private static Comment? FindInMemory(long commentId)
    {
        lock (_lock)
        {
            return _inMemoryStore.FirstOrDefault(c => c.Id == commentId);
        }
    }

    private static void SaveInMemory(Comment comment)
    {
        lock (_lock)
        {
            comment.GetType().GetProperty(nameof(comment.Id))!.SetValue(comment, _nextId++);
            _inMemoryStore.Add(comment);
        }
    }

    private static Result<CommentResponseDto> MapToResponseDtoInMemory(Comment comment)
    {
        var repliesCount = _inMemoryStore.Count(c =>
            !c.IsDeleted && c.ParentCommentId == comment.Id);

        return Result<CommentResponseDto>.Success(new CommentResponseDto(
            comment.Id,
            comment.PostId,
            comment.AuthorId,
            string.Empty,
            null,
            comment.Content,
            comment.IsEdited,
            comment.CreatedAt,
            comment.UpdatedAt,
            comment.ParentCommentId,
            repliesCount
        ));
    }

    private async Task<Result<CommentResponseDto>> MapToResponseDtoAsync(Comment comment)
    {
        var user = await _profileService!.GetByIdAsync(comment.AuthorId);
        var repliesCount = await _commentRepository!.CountAsync(c => c.ParentCommentId == comment.Id);

        var dto = new CommentResponseDto(
            comment.Id,
            comment.PostId,
            comment.AuthorId,
            user is not null ? $"{user.FirstName} {user.LastName}" : string.Empty,
            user?.ProfilePicturePath,
            comment.Content,
            comment.IsEdited,
            comment.CreatedAt,
            comment.UpdatedAt,
            comment.ParentCommentId,
            repliesCount
        );

        return Result<CommentResponseDto>.Success(dto);
    }

    private async Task<Dictionary<string, UserResponseDto>> GetUsersDictionaryAsync(IReadOnlyCollection<Comment> comments)
    {
        var authorIds = comments.Select(c => c.AuthorId).Distinct().ToList();
        var userTasks = authorIds.Select(id => _profileService!.GetByIdAsync(id));
        var users = await Task.WhenAll(userTasks);

        return users
            .Where(u => u is not null)
            .ToDictionary(u => u!.Id)!;
    }

    private static CommentTreeDto BuildCommentTreeDto(
        Comment comment,
        IReadOnlyCollection<Comment> allComments,
        Dictionary<string, UserResponseDto> userDict)
    {
        var dto = MapToFlatDto(comment, allComments, userDict);
        var replies = allComments
            .Where(c => c.ParentCommentId == comment.Id)
            .OrderBy(c => c.CreatedAt)
            .Select(reply => BuildCommentTreeDto(reply, allComments, userDict))
            .ToList();

        return new CommentTreeDto(dto, replies);
    }

    private static CommentResponseDto MapToFlatDto(
        Comment comment,
        IReadOnlyCollection<Comment> allComments,
        Dictionary<string, UserResponseDto> userDict)
    {
        var repliesCount = allComments.Count(c => c.ParentCommentId == comment.Id);
        var dto = new CommentResponseDto(
            comment.Id,
            comment.PostId,
            comment.AuthorId,
            string.Empty,
            null,
            comment.Content,
            comment.IsEdited,
            comment.CreatedAt,
            comment.UpdatedAt,
            comment.ParentCommentId,
            repliesCount
        );

        if (userDict.TryGetValue(comment.AuthorId, out var user))
        {
            dto = dto with
            {
                AuthorName = $"{user.FirstName} {user.LastName}",
                AuthorProfilePicture = user.ProfilePicturePath
            };
        }

        return dto;
    }
}
