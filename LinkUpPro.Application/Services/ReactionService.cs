using LinkUpPro.Application.DTOs.Reaction.Requests;
using LinkUpPro.Application.DTOs.Reaction.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.Interfaces.Repositories;
using Mapster;

namespace LinkUpPro.Application.Services;

public sealed class ReactionService : IReactionService
{
    private readonly IReactionRepository _reactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostRepository _postRepository;
    private readonly IProfileService _profileService;
    private readonly INotificationRepository _notificationRepository;

    public ReactionService(
        IReactionRepository reactionRepository,
        IUnitOfWork unitOfWork,
        IPostRepository postRepository,
        IProfileService profileService,
        INotificationRepository notificationRepository
    )
    {
        _reactionRepository = reactionRepository;
        _unitOfWork = unitOfWork;
        _postRepository = postRepository;
        _profileService = profileService;
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<ReactionResponseDto?>> ReactAsync(
        string userId,
        CreateReactionRequest request
    )
    {
        var reactionType = (ReactionType)request.Type;
        if (!Enum.IsDefined(reactionType))
            return Result<ReactionResponseDto?>.Failure(
                new DomainError("Reaction.InvalidType", "El tipo de reaccion no es valido.")
            );

        var existing = await _reactionRepository.GetByPostAndUserAsync(request.PostId, userId);

        if (existing is not null)
        {
            if (existing.Type == reactionType)
            {
                existing.MarkAsDeleted();
                _reactionRepository.Update(existing);
                await _unitOfWork.SaveChangesAsync();
                return Result<ReactionResponseDto?>.Success(null);
            }

            var changeResult = existing.ChangeTo(reactionType);
            if (changeResult.IsFailure)
                return Result<ReactionResponseDto?>.Failure(changeResult.Errors);

            _reactionRepository.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            await CreateReactionNotificationAsync(request.PostId, userId, reactionType);

            return Result<ReactionResponseDto?>.Success(existing.Adapt<ReactionResponseDto>());
        }

        var createResult = Reaction.Create(request.PostId, userId, reactionType);
        if (createResult.IsFailure)
            return Result<ReactionResponseDto?>.Failure(createResult.Errors);

        await _reactionRepository.AddAsync(createResult.Value);
        await _unitOfWork.SaveChangesAsync();

        await CreateReactionNotificationAsync(request.PostId, userId, reactionType);

        return Result<ReactionResponseDto?>.Success(
            createResult.Value.Adapt<ReactionResponseDto>()
        );
    }

    public async Task<Result> DeleteAsync(string userId, long postId)
    {
        var reaction = await _reactionRepository.GetByPostAndUserAsync(postId, userId);

        if (reaction is null)
            return Result.Failure(new DomainError("Reaction.NotFound", "La reaccion no existe."));

        reaction.MarkAsDeleted();
        _reactionRepository.Update(reaction);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<ReactionCountsDto>> GetCountsAsync(long postId)
    {
        var counts = await _reactionRepository.GetCountsByPostAsync(postId);
        return Result<ReactionCountsDto>.Success(
            new ReactionCountsDto(counts.Likes, counts.Dislikes)
        );
    }

    public async Task<int?> GetUserReactionAsync(string userId, long postId)
    {
        var reaction = await _reactionRepository.GetByPostAndUserAsync(postId, userId);
        return reaction?.Type switch
        {
            ReactionType.Like => (int)ReactionType.Like,
            ReactionType.Dislike => (int)ReactionType.Dislike,
            _ => null,
        };
    }

    private async Task CreateReactionNotificationAsync(
        long postId,
        string actorId,
        ReactionType reactionType
    )
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post is null || post.AuthorId == actorId)
            return;

        var actor = await _profileService.GetByIdAsync(actorId);
        var actorName = actor is null ? "Alguien" : $"{actor.FirstName} {actor.LastName}".Trim();

        var notifResult = Notification.CreateReaction(
            recipientId: post.AuthorId,
            actorId: actorId,
            postId: postId,
            actorUserName: actorName,
            reactionType: reactionType
        );

        if (notifResult.IsSuccess)
            await _notificationRepository.AddAsync(notifResult.Value);
    }
}
