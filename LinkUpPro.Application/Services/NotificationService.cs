using LinkUpPro.Application.DTOs.Notification.Requests;
using LinkUpPro.Application.DTOs.Notification.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.Interfaces.Repositories;

namespace LinkUpPro.Application.Services;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository? _notificationRepository;
    private readonly IUnitOfWork? _unitOfWork;

    private static readonly List<Notification> _inMemoryStore = new(SeedStore());
    private static readonly object _lock = new();

    private static IEnumerable<Notification> SeedStore()
    {
        var notification = Notification.CreateComment("user1", "actor1", 1, "Actor1").Value;
        typeof(Notification).GetProperty(nameof(Notification.Id))!.SetValue(notification, 1L);
        return [notification];
    }

    public NotificationService() : this(null, null) { }

    public NotificationService(
        INotificationRepository? notificationRepository,
        IUnitOfWork? unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<NotificationResponseDto>> GetNotificationsAsync(
        string userId, bool? unreadOnly = null, int page = 1, int pageSize = 20)
    {
        if (_notificationRepository is not null)
        {
            var options = new QueryOptions<Notification>
            {
                Skip = (page - 1) * pageSize,
                Take = pageSize,
                OrderBy = q => q.OrderByDescending(n => n.CreatedAt)
            };

            if (unreadOnly == true)
                options.Filter = n => !n.IsRead;

            var notifications = await _notificationRepository.GetByRecipientAsync(userId, options);
            var totalCount = unreadOnly == true
                ? await _notificationRepository.GetUnreadCountAsync(userId)
                : await _notificationRepository.CountAsync(n => n.RecipientId == userId);

            var items = notifications.Select(n => new NotificationResponseDto(
                n.Id, n.ActorId, string.Empty, null,
                n.Type, n.Message, n.CreatedAt, n.IsRead, n.RelatedPostId
            )).ToList().AsReadOnly();

            return new PagedResult<NotificationResponseDto>(items, totalCount, page, pageSize);
        }

        lock (_lock)
        {
            var filtered = _inMemoryStore
                .Where(n => n.IsForUser(userId))
                .Where(n => unreadOnly != true || !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            var totalCount = filtered.Count;
            var paged = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationResponseDto(
                    n.Id, n.ActorId, string.Empty, null,
                    n.Type, n.Message, n.CreatedAt, n.IsRead, n.RelatedPostId
                ))
                .ToList().AsReadOnly();

            return new PagedResult<NotificationResponseDto>(paged, totalCount, page, pageSize);
        }
    }

    public async Task<UnreadCountDto> GetUnreadCountAsync(string userId)
    {
        if (_notificationRepository is not null)
        {
            var count = await _notificationRepository.GetUnreadCountAsync(userId);
            return new UnreadCountDto(count);
        }

        lock (_lock)
        {
            var count = _inMemoryStore.Count(n => n.IsForUser(userId) && !n.IsRead);
            return new UnreadCountDto(count);
        }
    }

    public async Task<Result> MarkAsReadAsync(string userId, MarkAsReadRequest request)
    {
        if (_notificationRepository is not null)
        {
            var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);
            if (notification is null)
                return Result.Success();

            if (!notification.IsForUser(userId))
                return Result.Failure(new DomainError("Notification.NotOwner", "No tienes permiso para marcar esta notificacion."));

            await _notificationRepository.MarkAsReadAsync(request.NotificationId, userId);
            await _unitOfWork!.SaveChangesAsync();
            return Result.Success();
        }

        lock (_lock)
        {
            var notification = _inMemoryStore.FirstOrDefault(n => n.Id == request.NotificationId);
            if (notification is null)
                return Result.Success();

            if (!notification.IsForUser(userId))
                return Result.Failure(new DomainError("Notification.NotOwner", "No tienes permiso para marcar esta notificacion."));

            notification.MarkAsRead();
            return Result.Success();
        }
    }

    public async Task<Result> MarkAllAsReadAsync(string userId)
    {
        if (_notificationRepository is not null)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);
            await _unitOfWork!.SaveChangesAsync();
            return Result.Success();
        }

        lock (_lock)
        {
            foreach (var notification in _inMemoryStore.Where(n => n.IsForUser(userId) && !n.IsRead))
                notification.MarkAsRead();
            return Result.Success();
        }
    }
}
