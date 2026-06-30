using LinkUpPro.Application.DTOs.FriendRequest.Requests;
using LinkUpPro.Application.DTOs.FriendRequest.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Friendship;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.Interfaces.Repositories;

namespace LinkUpPro.Application.Services;

public sealed class FriendRequestService : IFriendRequestService
{
    private readonly IFriendRequestRepository _friendRequestRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IProfileService _profileService;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FriendRequestService(
        IFriendRequestRepository friendRequestRepository,
        IFriendshipRepository friendshipRepository,
        IProfileService profileService,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork
    )
    {
        _friendRequestRepository = friendRequestRepository;
        _friendshipRepository = friendshipRepository;
        _profileService = profileService;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<PendingRequestDto>> GetPendingRequestsAsync(
        string userId,
        int page = 1,
        int pageSize = 20
    )
    {
        var options = new QueryOptions<FriendRequest>
        {
            Skip = (page - 1) * pageSize,
            Take = pageSize,
            OrderBy = q => q.OrderByDescending(r => r.SentAt),
            IsTracking = false,
        };

        var requests = await _friendRequestRepository.GetPendingReceivedAsync(userId, options);
        var total = await _friendRequestRepository.CountAsync(r =>
            r.ReceiverId == userId && r.Status == FriendRequestStatus.Pending
        );

        if (requests.Count == 0)
            return new PagedResult<PendingRequestDto>([], total, page, pageSize);

        var senderIds = requests.Select(r => r.SenderId).Distinct().ToList();
        var commonCounts = await _friendshipRepository.GetCommonFriendsCountForUsersAsync(
            userId,
            senderIds
        );

        var userDict = await _profileService.GetByIdsAsync(senderIds);

        var items = new List<PendingRequestDto>(requests.Count);
        foreach (var req in requests)
        {
            var commonCount = commonCounts.GetValueOrDefault(req.SenderId, 0);
            userDict.TryGetValue(req.SenderId, out var sender);

            items.Add(
                new PendingRequestDto(
                    req.Id,
                    req.SenderId,
                    sender is null ? string.Empty : $"{sender.FirstName} {sender.LastName}".Trim(),
                    sender?.UserName ?? string.Empty,
                    sender?.ProfilePicturePath,
                    req.SentAt,
                    commonCount
                )
            );
        }

        return new PagedResult<PendingRequestDto>(items, total, page, pageSize);
    }

    public async Task<int> GetPendingCountAsync(string userId)
    {
        return await _friendRequestRepository.CountAsync(r =>
            r.ReceiverId == userId && r.Status == FriendRequestStatus.Pending
        );
    }

    public async Task<PagedResult<SentRequestDto>> GetSentRequestsAsync(
        string userId,
        int page = 1,
        int pageSize = 20
    )
    {
        var options = new QueryOptions<FriendRequest>
        {
            Skip = (page - 1) * pageSize,
            Take = pageSize,
            OrderBy = q => q.OrderByDescending(r => r.SentAt),
            IsTracking = false,
        };

        var requests = await _friendRequestRepository.GetVisibleSentHistoryAsync(userId, options);
        var total = await _friendRequestRepository.CountAsync(r =>
            r.SenderId == userId
            && r.IsVisibleForSender
            && r.Status != FriendRequestStatus.Pending
            && r.Status != FriendRequestStatus.Canceled
        );

        if (requests.Count == 0)
            return new PagedResult<SentRequestDto>([], total, page, pageSize);

        var receiverIds = requests.Select(r => r.ReceiverId).Distinct().ToList();
        var commonCounts = await _friendshipRepository.GetCommonFriendsCountForUsersAsync(
            userId,
            receiverIds
        );

        var userDict = await _profileService.GetByIdsAsync(receiverIds);
        var sender = await _profileService.GetByIdAsync(userId);
        var senderName = sender is null
            ? string.Empty
            : $"{sender.FirstName} {sender.LastName}".Trim();

        var items = new List<SentRequestDto>(requests.Count);
        foreach (var req in requests)
        {
            var commonCount = commonCounts.GetValueOrDefault(req.ReceiverId, 0);
            userDict.TryGetValue(req.ReceiverId, out var receiver);

            items.Add(
                new SentRequestDto(
                    req.Id,
                    req.ReceiverId,
                    receiver is null
                        ? string.Empty
                        : $"{receiver.FirstName} {receiver.LastName}".Trim(),
                    receiver?.UserName ?? string.Empty,
                    receiver?.ProfilePicturePath,
                    commonCount,
                    req.SentAt,
                    req.Status,
                    req.RespondedAt,
                    req.IsVisibleForSender,
                    senderName
                )
            );
        }

        return new PagedResult<SentRequestDto>(items, total, page, pageSize);
    }

    public async Task<Result> SendAsync(string senderId, SendFriendRequestRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ReceiverId))
            return Result.Failure(
                new DomainError(
                    "FriendRequest.ReceiverRequired",
                    "El usuario receptor es requerido."
                )
            );

        if (senderId == request.ReceiverId)
            return Result.Failure(
                new DomainError(
                    "FriendRequest.SelfRequestNotAllowed",
                    "Un usuario no puede enviarse una solicitud a si mismo."
                )
            );

        var receiver = await _profileService.GetByIdAsync(request.ReceiverId);
        if (receiver is null)
            return Result.Failure(
                new DomainError("User.NotFound", "El usuario receptor no existe.")
            );

        if (!receiver.IsActive)
            return Result.Failure(
                new DomainError(
                    "User.Inactive",
                    "No se puede enviar la solicitud porque el usuario se encuentra inactivo."
                )
            );

        if (await _friendshipRepository.AreFriendsAsync(senderId, request.ReceiverId))
            return Result.Failure(
                new DomainError(
                    "FriendRequest.AlreadyFriends",
                    "Este usuario ya forma parte de su lista de amigos."
                )
            );

        if (await _friendRequestRepository.ExistsPendingBetweenAsync(senderId, request.ReceiverId))
            return Result.Failure(
                new DomainError(
                    "FriendRequest.PendingExists",
                    "Ya existe una solicitud de amistad pendiente entre estos usuarios."
                )
            );

        var creationResult = FriendRequest.Create(senderId, request.ReceiverId);
        if (creationResult.IsFailure)
            return Result.Failure(creationResult.Errors);

        var sender = await _profileService.GetByIdAsync(senderId);
        var senderName = sender is null
            ? "Alguien"
            : $"{sender.FirstName} {sender.LastName}".Trim();

        var notifResult = Notification.CreateFriendRequestSent(
            recipientId: request.ReceiverId,
            actorId: senderId,
            requestId: creationResult.Value.Id,
            actorUserName: senderName
        );

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _friendRequestRepository.AddAsync(creationResult.Value);

            if (notifResult.IsSuccess)
                await _notificationRepository.AddAsync(notifResult.Value);

            await _unitOfWork.CommitAsync();
            return Result.Success();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<Result> AcceptAsync(string receiverId, AcceptFriendRequestRequest request)
    {
        var friendRequest = await _friendRequestRepository.GetByIdForReceiverAsync(
            request.RequestId,
            receiverId
        );
        if (friendRequest is null)
            return Result.Failure(
                new DomainError(
                    "FriendRequest.NotFound",
                    "La solicitud no fue encontrada o no le pertenece."
                )
            );

        var acceptResult = friendRequest.Accept(receiverId);
        if (acceptResult.IsFailure)
            return acceptResult;

        var existingFriendship = await _friendshipRepository.GetFriendshipBetweenAsync(
            friendRequest.SenderId,
            friendRequest.ReceiverId
        );

        if (existingFriendship is not null)
        {
            if (existingFriendship.IsDeleted)
                existingFriendship.Reactivate();
        }
        else
        {
            var friendshipResult = Friendship.Create(
                friendRequest.SenderId,
                friendRequest.ReceiverId
            );
            if (friendshipResult.IsFailure)
                return Result.Failure(friendshipResult.Errors);

            await _friendshipRepository.AddAsync(friendshipResult.Value);
        }

        var receiver = await _profileService.GetByIdAsync(receiverId);
        var receiverName = receiver is null
            ? "Alguien"
            : $"{receiver.FirstName} {receiver.LastName}".Trim();

        var notifResult = Notification.CreateFriendRequestAccepted(
            recipientId: friendRequest.SenderId,
            actorId: receiverId,
            requestId: friendRequest.Id,
            actorUserName: receiverName
        );

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            _friendRequestRepository.Update(friendRequest);

            if (notifResult.IsSuccess)
                await _notificationRepository.AddAsync(notifResult.Value);

            await _unitOfWork.CommitAsync();
            return Result.Success();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<Result> RejectAsync(string receiverId, RejectFriendRequestRequest request)
    {
        var friendRequest = await _friendRequestRepository.GetByIdForReceiverAsync(
            request.RequestId,
            receiverId
        );
        if (friendRequest is null)
            return Result.Failure(
                new DomainError(
                    "FriendRequest.NotFound",
                    "La solicitud no fue encontrada o no le pertenece."
                )
            );

        var rejectResult = friendRequest.Reject(receiverId);
        if (rejectResult.IsFailure)
            return rejectResult;

        var receiver = await _profileService.GetByIdAsync(receiverId);
        var receiverName = receiver is null
            ? "Alguien"
            : $"{receiver.FirstName} {receiver.LastName}".Trim();

        var notifResult = Notification.CreateFriendRequestRejected(
            recipientId: friendRequest.SenderId,
            actorId: receiverId,
            requestId: friendRequest.Id,
            actorUserName: receiverName
        );

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            _friendRequestRepository.Update(friendRequest);

            if (notifResult.IsSuccess)
                await _notificationRepository.AddAsync(notifResult.Value);

            await _unitOfWork.CommitAsync();
            return Result.Success();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<Result> CancelAsync(string senderId, CancelFriendRequestRequest request)
    {
        var friendRequest = await _friendRequestRepository.GetByIdForSenderAsync(
            request.RequestId,
            senderId
        );
        if (friendRequest is null)
            return Result.Failure(
                new DomainError(
                    "FriendRequest.NotFound",
                    "La solicitud no fue encontrada o no le pertenece."
                )
            );

        var cancelResult = friendRequest.Cancel(senderId);
        if (cancelResult.IsFailure)
            return cancelResult;

        _friendRequestRepository.Update(friendRequest);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> DeleteFromHistoryAsync(
        string senderId,
        DeleteFromHistoryRequest request
    )
    {
        var friendRequest = await _friendRequestRepository.GetByIdForSenderAsync(
            request.RequestId,
            senderId
        );
        if (friendRequest is null)
            return Result.Failure(
                new DomainError(
                    "FriendRequest.NotFound",
                    "La solicitud no fue encontrada o no le pertenece."
                )
            );

        var hideResult = friendRequest.HideFromSenderHistory(senderId);
        if (hideResult.IsFailure)
            return hideResult;

        _friendRequestRepository.Update(friendRequest);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<PagedResult<AvailableUserDto>> SearchAvailableUsersAsync(
        string userId,
        string? search = null,
        int page = 1,
        int pageSize = 20
    )
    {
        var options = new QueryOptions<UserSearchResult>
        {
            Skip = (page - 1) * pageSize,
            Take = pageSize,
            OrderBy = q => q.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
            IsTracking = false,
        };

        var users = await _friendRequestRepository.SearchAvailableUsersPagedAsync(
            userId,
            search,
            options
        );
        var total = await _friendRequestRepository.CountAvailableUsersAsync(userId, search);

        if (users.Count == 0)
            return new PagedResult<AvailableUserDto>([], total, page, pageSize);

        var userIds = users.Select(u => u.Id).ToList();
        var commonCounts = await _friendshipRepository.GetCommonFriendsCountForUsersAsync(
            userId,
            userIds
        );

        var items = users
            .Select(user => new AvailableUserDto(
                user.Id,
                $"{user.FirstName} {user.LastName}".Trim(),
                user.UserName,
                user.ProfilePicturePath,
                commonCounts.GetValueOrDefault(user.Id, 0)
            ))
            .ToList();

        return new PagedResult<AvailableUserDto>(items, total, page, pageSize);
    }
}
