namespace LinkUpPro.Application.DTOs.FriendRequest.Responses;

public record AvailableUserDto(
    string Id,
    string Name,
    string UserName,
    string? ProfilePicturePath,
    int CommonFriendsCount
);
