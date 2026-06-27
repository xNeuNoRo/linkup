using LinkUpPro.Application.DTOs.Friendship.Requests;
using LinkUpPro.Application.DTOs.Friendship.Responses;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.Interfaces.Services;

public interface IFriendshipService
{
    Task<PagedResult<FriendListItemDto>> GetFriendsAsync(
        string userId,
        string? search = null,
        int page = 1,
        int pageSize = 20
    );

    Task<Result<CommonFriendsDto>> GetCommonFriendsAsync(string userId, string targetUserId);

    Task<Result<FriendshipResponseDto>> GetFriendshipAsync(string userId, string friendId);

    Task<Result> DeleteAsync(string userId, string friendId);

    Task<int> GetActiveFriendsCountAsync(string userId);

    Task<int> GetAvailablePostsCountAsync(string userId);
}
