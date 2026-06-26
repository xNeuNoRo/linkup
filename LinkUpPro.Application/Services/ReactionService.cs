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
    private readonly IReactionRepository? _reactionRepository;
    private readonly IUnitOfWork? _unitOfWork;

    private sealed record StoredReaction(long Id, long PostId, string UserId, int Type, DateTimeOffset CreatedAt, DateTimeOffset? DeletedAt)
    {
        public bool IsDeleted => DeletedAt is not null;
    }

    private readonly List<StoredReaction> _inMemoryStore = [];
    private long _nextId = 1;
    private readonly object _lock = new();

    public ReactionService(
        IReactionRepository? reactionRepository,
        IUnitOfWork? unitOfWork,
        object? _ = null)
    {
        _reactionRepository = reactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReactionResponseDto>> ReactAsync(string userId, CreateReactionRequest request)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<ReactionResponseDto>.Failure(new DomainError("Reaction.UserRequired", "El usuario es requerido."));

        if (_reactionRepository is not null)
        {
            var reactionType = (ReactionType)request.Type;
            if (!Enum.IsDefined(reactionType))
                return Result<ReactionResponseDto>.Failure(new DomainError("Reaction.InvalidType", "El tipo de reaccion no es valido."));

            var existing = await _reactionRepository.GetByPostAndUserAsync(request.PostId, userId);

            if (existing is not null)
            {
                if (existing.Type == reactionType)
                {
                    existing.MarkAsDeleted();
                    _reactionRepository.Update(existing);
                    await _unitOfWork!.SaveChangesAsync();
                    return Result<ReactionResponseDto>.Success(null!);
                }

                var changeResult = existing.ChangeTo(reactionType);
                if (changeResult.IsFailure)
                    return Result<ReactionResponseDto>.Failure(changeResult.Errors);

                _reactionRepository.Update(existing);
                await _unitOfWork!.SaveChangesAsync();

                return Result<ReactionResponseDto>.Success(existing.Adapt<ReactionResponseDto>());
            }

            var createResult = Reaction.Create(request.PostId, userId, reactionType);
            if (createResult.IsFailure)
                return Result<ReactionResponseDto>.Failure(createResult.Errors);

            await _reactionRepository.AddAsync(createResult.Value);
            await _unitOfWork!.SaveChangesAsync();

            return Result<ReactionResponseDto>.Success(createResult.Value.Adapt<ReactionResponseDto>());
        }

        lock (_lock)
        {
            var existing = _inMemoryStore.FirstOrDefault(r =>
                r.PostId == request.PostId && r.UserId == userId && !r.IsDeleted);

            if (existing is not null)
            {
                if (existing.Type == request.Type)
                {
                    _inMemoryStore.Remove(existing);
                    _inMemoryStore.Add(existing with { DeletedAt = DateTimeOffset.UtcNow });
                    return Result<ReactionResponseDto>.Success(null!);
                }

                _inMemoryStore.Remove(existing);
                _inMemoryStore.Add(existing with { Type = request.Type });
                return Result<ReactionResponseDto>.Success(
                    new ReactionResponseDto(existing.Id, request.PostId, userId, request.Type, existing.CreatedAt));
            }

            var reaction = new StoredReaction(_nextId++, request.PostId, userId, request.Type, DateTimeOffset.UtcNow, null);
            _inMemoryStore.Add(reaction);

            return Result<ReactionResponseDto>.Success(
                new ReactionResponseDto(reaction.Id, reaction.PostId, reaction.UserId, reaction.Type, reaction.CreatedAt));
        }
    }

    public async Task<Result> DeleteAsync(string userId, long postId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure(new DomainError("Reaction.UserRequired", "El usuario es requerido."));

        if (_reactionRepository is not null)
        {
            var reaction = await _reactionRepository.GetByPostAndUserAsync(postId, userId);

            if (reaction is null)
                return Result.Failure(new DomainError("Reaction.NotFound", "La reaccion no existe."));

            reaction.MarkAsDeleted();
            _reactionRepository.Update(reaction);
            await _unitOfWork!.SaveChangesAsync();

            return Result.Success();
        }

        lock (_lock)
        {
            var reaction = _inMemoryStore.FirstOrDefault(r =>
                r.PostId == postId && r.UserId == userId && !r.IsDeleted);

            if (reaction is null)
                return Result.Success();

            _inMemoryStore.Remove(reaction);
            _inMemoryStore.Add(reaction with { DeletedAt = DateTimeOffset.UtcNow });
            return Result.Success();
        }
    }

    public async Task<Result<ReactionCountsDto>> GetCountsAsync(long postId)
    {
        if (_reactionRepository is not null)
        {
            var counts = await _reactionRepository.GetCountsByPostAsync(postId);
            return Result<ReactionCountsDto>.Success(new ReactionCountsDto(counts.Likes, counts.Dislikes));
        }

        lock (_lock)
        {
            var likes = _inMemoryStore.Count(r => r.PostId == postId && r.Type == 1 && !r.IsDeleted);
            var dislikes = _inMemoryStore.Count(r => r.PostId == postId && r.Type == 0 && !r.IsDeleted);
            return Result<ReactionCountsDto>.Success(new ReactionCountsDto(likes, dislikes));
        }
    }

    public async Task<int?> GetUserReactionAsync(string userId, long postId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        if (_reactionRepository is not null)
        {
            var reaction = await _reactionRepository.GetByPostAndUserAsync(postId, userId);
            return reaction?.Type switch
            {
                ReactionType.Like => (int)ReactionType.Like,
                ReactionType.Dislike => (int)ReactionType.Dislike,
                _ => null
            };
        }

        lock (_lock)
        {
            var reaction = _inMemoryStore.FirstOrDefault(r =>
                r.PostId == postId && r.UserId == userId && !r.IsDeleted);
            return reaction?.Type switch
            {
                1 => 1,
                0 => 0,
                _ => null
            };
        }
    }
}
