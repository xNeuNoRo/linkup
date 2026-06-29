using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Interfaces;
using LinkUpPro.Domain.Interfaces.Repositories;
using LinkUpPro.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Identity.Services;

public sealed class UserDirectory : IUserDirectory
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IFriendshipRepository _friendshipRepository;

    public UserDirectory(
        UserManager<AppUser> userManager,
        IFriendshipRepository friendshipRepository
    )
    {
        _userManager = userManager;
        _friendshipRepository = friendshipRepository;
    }

    public async Task<IReadOnlyCollection<UserSearchResult>> SearchAvailableUsersPagedAsync(
        string currentUserId,
        string? searchTerm,
        QueryOptions<UserSearchResult> options,
        CancellationToken cancellationToken = default
    )
    {
        var friendIds = await _friendshipRepository.GetActiveFriendIdsAsync(
            currentUserId,
            cancellationToken
        );
        var pendingIds = await _friendshipRepository.GetPendingRequestUserIdsAsync(
            currentUserId,
            cancellationToken
        );

        var excludedIds = new HashSet<string>(friendIds) { currentUserId };
        foreach (var id in pendingIds)
            excludedIds.Add(id);

        var query = _userManager
            .Users.Where(u => u.IsActive && !excludedIds.Contains(u.Id))
            .Select(u => new UserSearchResult(
                u.Id,
                u.UserName ?? string.Empty,
                u.FirstName,
                u.LastName,
                u.Email ?? string.Empty,
                u.ProfilePicturePath
            ));

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(u =>
                u.UserName.Contains(term)
                || u.FirstName.Contains(term)
                || u.LastName.Contains(term)
                || u.Email.Contains(term)
            );
        }

        return await ApplyUserOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAvailableUsersAsync(
        string currentUserId,
        string? searchTerm,
        CancellationToken cancellationToken = default
    )
    {
        var friendIds = await _friendshipRepository.GetActiveFriendIdsAsync(
            currentUserId,
            cancellationToken
        );
        var pendingIds = await _friendshipRepository.GetPendingRequestUserIdsAsync(
            currentUserId,
            cancellationToken
        );

        var excludedIds = new HashSet<string>(friendIds) { currentUserId };
        foreach (var id in pendingIds)
            excludedIds.Add(id);

        var query = _userManager.Users.Where(u => u.IsActive && !excludedIds.Contains(u.Id));

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(u =>
                (u.UserName ?? string.Empty).Contains(term)
                || u.FirstName.Contains(term)
                || u.LastName.Contains(term)
                || (u.Email ?? string.Empty).Contains(term)
            );
        }

        return await query.CountAsync(cancellationToken);
    }

    private static IQueryable<UserSearchResult> ApplyUserOptionsToQuery(
        IQueryable<UserSearchResult> query,
        QueryOptions<UserSearchResult> options
    )
    {
        if (!options.IsTracking)
            query = query.AsNoTracking();

        if (options.Filter is not null)
            query = query.Where(options.Filter);

        if (options.OrderBy is not null)
            query = options.OrderBy(query);

        if (options.Skip.HasValue)
            query = query.Skip(options.Skip.Value);

        if (options.Take.HasValue)
            query = query.Take(options.Take.Value);

        return query;
    }
}
