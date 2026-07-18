namespace LinkUpPro.Application.DTOs.Friendship.Responses;

public record FriendListItemDto(
    string FriendId,
    string FriendName,
    string FriendUserName,
    string? FriendProfilePicturePath,
    int CommonFriendsCount
);
