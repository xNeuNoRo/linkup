namespace LinkUpPro.Application.DTOs.Reaction.Responses;

public record ReactionResponseDto(
    long Id, long PostId, string UserId, int Type, DateTimeOffset CreatedAt);
