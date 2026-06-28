using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.DTOs.Reaction.Responses;

public record ReactionResponseDto(
    long Id, long PostId, string UserId, ReactionType Type, DateTimeOffset CreatedAt);
