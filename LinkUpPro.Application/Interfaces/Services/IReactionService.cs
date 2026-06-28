using LinkUpPro.Application.DTOs.Reaction.Requests;
using LinkUpPro.Application.DTOs.Reaction.Responses;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.Interfaces.Services;

public interface IReactionService
{
    Task<Result<ReactionResponseDto?>> ReactAsync(string userId, CreateReactionRequest request);

    Task<Result> DeleteAsync(string userId, long postId);

    Task<Result<ReactionCountsDto>> GetCountsAsync(long postId);

    Task<ReactionType?> GetUserReactionAsync(string userId, long postId);
}
