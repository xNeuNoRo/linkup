namespace LinkUpPro.Application.DTOs.Friendship.Responses;

public record FriendshipResponseDto(
    long Id, string UserId, string FriendId, DateTimeOffset CreatedAt, bool IsActive);
