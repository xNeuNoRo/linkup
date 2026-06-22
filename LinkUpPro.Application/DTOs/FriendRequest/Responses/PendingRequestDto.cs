namespace LinkUpPro.Application.DTOs.FriendRequest.Responses;

public record PendingRequestDto(
    long Id, string SenderId, string SenderName, string SenderUserName,
    string? SenderProfilePicture, DateTimeOffset SentAt, int CommonFriendsCount);
