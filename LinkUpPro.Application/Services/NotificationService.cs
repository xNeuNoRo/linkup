using LinkUpPro.Application.DTOs.Notification.Requests;
using LinkUpPro.Application.DTOs.Notification.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.Interfaces.Repositories;
using Mapster;

namespace LinkUpPro.Application.Services;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IProfileService _profileService;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(
        INotificationRepository notificationRepository,
        IProfileService profileService,
        IUnitOfWork unitOfWork
    )
    {
        _notificationRepository = notificationRepository;
        _profileService = profileService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<NotificationResponseDto>> GetNotificationsAsync(
        string userId,
        bool? unreadOnly = null,
        int page = 1,
        int pageSize = 20
    )
    {
        var options = new QueryOptions<Notification>
        {
            Skip = (page - 1) * pageSize,
            Take = pageSize,
            OrderBy = q => q.OrderByDescending(n => n.CreatedAt),
            IsTracking = false,
        };

        if (unreadOnly == true)
            options.Filter = n => !n.IsRead;

        var notifications = await _notificationRepository.GetByRecipientAsync(userId, options);

        var totalCount =
            unreadOnly == true
                ? await _notificationRepository.GetUnreadCountAsync(userId)
                : await _notificationRepository.CountAsync(n => n.RecipientId == userId);

        if (notifications.Count == 0)
            return new PagedResult<NotificationResponseDto>([], totalCount, page, pageSize);

        var actorIds = notifications.Select(n => n.ActorId).Distinct().ToList();
        var userDict = await _profileService.GetByIdsAsync(actorIds);

        var items = new List<NotificationResponseDto>(notifications.Count);
        foreach (var notification in notifications)
        {
            var dto = notification.Adapt<NotificationResponseDto>();
            if (userDict.TryGetValue(notification.ActorId, out var actor))
            {
                dto = dto with
                {
                    ActorName = $"{actor.FirstName} {actor.LastName}".Trim(),
                    ActorProfilePicture = actor.ProfilePicturePath,
                };
            }
            items.Add(dto);
        }

        return new PagedResult<NotificationResponseDto>(items, totalCount, page, pageSize);
    }

    public async Task<UnreadCountDto> GetUnreadCountAsync(string userId)
    {
        var count = await _notificationRepository.GetUnreadCountAsync(userId);
        return new UnreadCountDto(count);
    }

    public async Task<Result> MarkAsReadAsync(string userId, MarkAsReadRequest request)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);
        if (notification is null)
            return Result.Success();

        if (!notification.IsForUser(userId))
            return Result.Failure(
                new DomainError(
                    "Notification.NotOwner",
                    "No tienes permiso para marcar esta notificacion."
                )
            );

        notification.MarkAsRead();
        _notificationRepository.Update(notification);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> MarkAllAsReadAsync(string userId)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }
}
