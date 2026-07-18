using LinkUpPro.Application.DTOs.FriendRequest.Requests;
using LinkUpPro.Application.DTOs.FriendRequest.Responses;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.Interfaces.Services;

public interface IFriendRequestService
{
    Task<PagedResult<PendingRequestDto>> GetPendingRequestsAsync(
        string userId, int page = 1, int pageSize = 20);

    Task<int> GetPendingCountAsync(string userId);

    Task<PagedResult<SentRequestDto>> GetSentRequestsAsync(
        string userId, int page = 1, int pageSize = 20);

    Task<Result> SendAsync(string senderId, SendFriendRequestRequest request);

    Task<Result> AcceptAsync(string receiverId, AcceptFriendRequestRequest request);

    Task<Result> RejectAsync(string receiverId, RejectFriendRequestRequest request);

    Task<Result> CancelAsync(string senderId, CancelFriendRequestRequest request);

    Task<Result> DeleteFromHistoryAsync(string senderId, DeleteFromHistoryRequest request);

    Task<PagedResult<AvailableUserDto>> SearchAvailableUsersAsync(
        string userId, string? search = null, int page = 1, int pageSize = 20);
}
