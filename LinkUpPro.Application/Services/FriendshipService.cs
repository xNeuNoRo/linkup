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

    public FriendshipService(
        IFriendshipRepository friendshipRepository,
        IProfileService profileService,
        IUnitOfWork unitOfWork)
    {
        _friendshipRepository = friendshipRepository;
        _profileService = profileService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<FriendListItemDto>> GetFriendsAsync(
        string userId, string? search = null, int page = 1, int pageSize = 20)
    {
        var friendIds = await _friendshipRepository.GetActiveFriendIdsAsync(userId);

        var users = new List<UserInfo>();
        foreach (var friendId in friendIds)
        {
            var userDto = await _profileService.GetByIdAsync(friendId);
            if (userDto is null)
                continue;

            users.Add(new UserInfo(
                userDto.Id,
                $"{userDto.FirstName} {userDto.LastName}".Trim(),
                userDto.UserName,
                userDto.ProfilePicturePath));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            users = users.Where(u =>
                u.UserName.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var total = users.Count;

        var ordered = users
            .OrderBy(u => u.FullName)
            .ToList();

        var paged = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<FriendListItemDto>(paged.Count);
        foreach (var user in paged)
        {
            var commonCount = await _friendshipRepository.GetCommonFriendsCountAsync(userId, user.Id);
            items.Add(new FriendListItemDto(
                user.Id,
                user.FullName,
                user.UserName,
                user.ProfilePicturePath,
                commonCount));
        }

        return new PagedResult<FriendListItemDto>(items, total, page, pageSize);
    }

    public async Task<Result<CommonFriendsDto>> GetCommonFriendsAsync(string userId, string targetUserId)
    {
        var commonIds = await _friendshipRepository.GetCommonFriendIdsAsync(userId, targetUserId);

        var friends = new List<FriendListItemDto>(commonIds.Count);
        foreach (var commonId in commonIds)
        {
            var userDto = await _profileService.GetByIdAsync(commonId);
            if (userDto is null)
                continue;

            var commonCount = await _friendshipRepository.GetCommonFriendsCountAsync(userId, commonId);
            friends.Add(new FriendListItemDto(
                commonId,
                $"{userDto.FirstName} {userDto.LastName}".Trim(),
                userDto.UserName,
                userDto.ProfilePicturePath,
                commonCount));
        }

        var dto = new CommonFriendsDto(friends.Count, friends);
        return Result<CommonFriendsDto>.Success(dto);
    }

    public async Task<Result<FriendshipResponseDto>> GetFriendshipAsync(string userId, string friendId)
    {
        var friendship = await _friendshipRepository.GetFriendshipBetweenAsync(userId, friendId);

        if (friendship is null)
        {
            return Result<FriendshipResponseDto>.Failure(
                new DomainError("Friendship.NotFound", "No existe una relacion de amistad entre estos usuarios."));
        }

        var friendResult = friendship.GetFriendId(userId);
        if (friendResult.IsFailure)
        {
            return Result<FriendshipResponseDto>.Failure(friendResult.Errors);
        }

        var dto = friendship.Adapt<FriendshipResponseDto>() with
        {
            UserId = userId,
            FriendId = friendResult.Value
        };

        return Result<FriendshipResponseDto>.Success(dto);
    }

    public async Task<Result> DeleteAsync(string userId, string friendId)
    {
        var friendship = await _friendshipRepository.GetFriendshipBetweenAsync(userId, friendId);

        if (friendship is null || !friendship.InvolvesUser(userId))
        {
            return Result.Failure(
                new DomainError("Friendship.NotFound", "No son amigos."));
        }

        if (friendship.IsDeleted)
        {
            return Result.Failure(
                new DomainError("Friendship.AlreadyDeleted", "Esta amistad ya ha sido eliminada."));
        }

        friendship.MarkAsDeleted();
        _friendshipRepository.Update(friendship);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    private sealed record UserInfo(string Id, string FullName, string UserName, string? ProfilePicturePath);
}
