using LinkUpPro.Application.DTOs.Post.Requests;
using LinkUpPro.Application.DTOs.Post.Responses;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.Interfaces.Services;

public interface IPostService
{
    Task<Result<PostResponseDto>> CreateAsync(string authorId, CreatePostRequest request);

    Task<Result<PostResponseDto>> GetByIdAsync(string requesterId, long postId);

    Task<Result<PostResponseDto>> UpdateAsync(string authorId, long postId, UpdatePostRequest request);

    Task<Result> DeleteAsync(string authorId, long postId);

    Task<PagedResult<PostListItemDto>> GetMyPostsAsync(string userId, PostFilterRequest filter);

    Task<PagedResult<PostListItemDto>> GetFriendsPostsAsync(string userId, PostFilterRequest filter);

    Task<PagedResult<PostListItemDto>> GetUserPostsAsync(string requesterId, string targetUserId, PostFilterRequest filter);

    Task<Result<PostStatsDto>> GetStatsAsync(string userId);
}
