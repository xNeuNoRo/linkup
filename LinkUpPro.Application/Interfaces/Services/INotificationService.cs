using LinkUpPro.Application.DTOs.Notification.Requests;
using LinkUpPro.Application.DTOs.Notification.Responses;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.Interfaces.Services;

public interface INotificationService
{
    Task<PagedResult<NotificationResponseDto>> GetNotificationsAsync(
        string userId, bool? unreadOnly = null, int page = 1, int pageSize = 20);

    Task<UnreadCountDto> GetUnreadCountAsync(string userId);

    Task<Result> MarkAsReadAsync(string userId, MarkAsReadRequest request);

    Task<Result> MarkAllAsReadAsync(string userId);
}
