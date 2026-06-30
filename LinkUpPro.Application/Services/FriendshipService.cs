using LinkUpPro.Application.DTOs.Friendship.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.Interfaces.Repositories;
using Mapster;

namespace LinkUpPro.Application.Services;

public sealed class FriendshipService : IFriendshipService
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IProfileService _profileService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPostRepository _postRepository;

    public FriendshipService(
        IFriendshipRepository friendshipRepository,
        IProfileService profileService,
        IUnitOfWork unitOfWork,
        IPostRepository postRepository
    )
    {
        _friendshipRepository = friendshipRepository;
        _profileService = profileService;
        _unitOfWork = unitOfWork;
        _postRepository = postRepository;
    }

    public async Task<PagedResult<FriendListItemDto>> GetFriendsAsync(
        string userId,
        string? search = null,
        int page = 1,
        int pageSize = 20
    )
    {
        var allFriendIds = await _friendshipRepository.GetActiveFriendIdsAsync(userId);

        if (allFriendIds.Count == 0)
            return new PagedResult<FriendListItemDto>([], 0, page, pageSize);

        IReadOnlyCollection<string> visibleIds = allFriendIds;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var userDict = await _profileService.GetByIdsAsync(allFriendIds);

            visibleIds = allFriendIds
                .Where(id =>
                    userDict.TryGetValue(id, out var u)
                    && (
                        u.UserName.Contains(term, StringComparison.OrdinalIgnoreCase)
                        || $"{u.FirstName} {u.LastName}"
                            .Trim()
                            .Contains(term, StringComparison.OrdinalIgnoreCase)
                    )
                )
                .ToList()
                .AsReadOnly();

            if (visibleIds.Count == 0)
                return new PagedResult<FriendListItemDto>([], 0, page, pageSize);
        }

        var total = visibleIds.Count;
        var pagedIds = visibleIds.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var pagedUserDict = await _profileService.GetByIdsAsync(pagedIds);

        var commonCounts = await _friendshipRepository.GetCommonFriendsCountForUsersAsync(
            userId,
            pagedIds
        );

        var items = pagedIds
            .Where(id => pagedUserDict.ContainsKey(id))
            .Select(id =>
            {
                var u = pagedUserDict[id];
                return new FriendListItemDto(
                    id,
                    $"{u.FirstName} {u.LastName}".Trim(),
                    u.UserName,
                    u.ProfilePicturePath,
                    commonCounts.GetValueOrDefault(id, 0)
                );
            })
            .OrderBy(f => f.FriendName)
            .ToList();

        return new PagedResult<FriendListItemDto>(items, total, page, pageSize);
    }

    public async Task<Result<CommonFriendsDto>> GetCommonFriendsAsync(
        string userId,
        string targetUserId
    )
    {
        var commonIds = await _friendshipRepository.GetCommonFriendIdsAsync(userId, targetUserId);
        if (commonIds.Count == 0)
        {
            return Result<CommonFriendsDto>.Success(new CommonFriendsDto(0, []));
        }

        var commonCounts = await _friendshipRepository.GetCommonFriendsCountForUsersAsync(
            userId,
            commonIds
        );

        var userDict = await _profileService.GetByIdsAsync(commonIds);

        var friends = new List<FriendListItemDto>(commonIds.Count);
        foreach (var commonId in commonIds)
        {
            if (!userDict.TryGetValue(commonId, out var user))
                continue;

            friends.Add(
                new FriendListItemDto(
                    commonId,
                    $"{user.FirstName} {user.LastName}".Trim(),
                    user.UserName,
                    user.ProfilePicturePath,
                    commonCounts.GetValueOrDefault(commonId, 0)
                )
            );
        }

        var dto = new CommonFriendsDto(friends.Count, friends);
        return Result<CommonFriendsDto>.Success(dto);
    }

    public async Task<Result<FriendshipResponseDto>> GetFriendshipAsync(
        string userId,
        string friendId
    )
    {
        var friendship = await _friendshipRepository.GetFriendshipBetweenAsync(userId, friendId);

        if (friendship is null)
        {
            return Result<FriendshipResponseDto>.Failure(
                new DomainError(
                    "Friendship.NotFound",
                    "No existe una relacion de amistad entre estos usuarios."
                )
            );
        }

        var friendResult = friendship.GetFriendId(userId);
        if (friendResult.IsFailure)
        {
            return Result<FriendshipResponseDto>.Failure(friendResult.Errors);
        }

        var dto = friendship.Adapt<FriendshipResponseDto>() with
        {
            UserId = userId,
            FriendId = friendResult.Value,
        };

        return Result<FriendshipResponseDto>.Success(dto);
    }

    public async Task<Result> DeleteAsync(string userId, string friendId)
    {
        var friendship = await _friendshipRepository.GetFriendshipBetweenAsync(userId, friendId);

        if (friendship is null || !friendship.InvolvesUser(userId))
        {
            return Result.Failure(new DomainError("Friendship.NotFound", "No son amigos."));
        }

        if (friendship.IsDeleted)
        {
            return Result.Failure(
                new DomainError("Friendship.AlreadyDeleted", "Esta amistad ya ha sido eliminada.")
            );
        }

        friendship.MarkAsDeleted();

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            _friendshipRepository.Update(friendship);
            await _unitOfWork.CommitAsync();
            return Result.Success();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<int> GetActiveFriendsCountAsync(string userId)
    {
        return await _friendshipRepository.GetActiveFriendsCountAsync(userId);
    }

    public async Task<int> GetAvailablePostsCountAsync(string userId)
    {
        var friendIds = await _friendshipRepository.GetActiveFriendIdsAsync(userId);
        if (friendIds.Count == 0)
            return 0;

        return await _postRepository.CountAvailableFriendsPostsAsync(userId);
    }
}
