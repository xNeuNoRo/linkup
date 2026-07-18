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

        // Build query on AppUser (before projection to avoid LINQ translation issues)
        var query = _userManager
            .Users.Where(u => u.IsActive && !excludedIds.Contains(u.Id));

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

        if (!options.IsTracking)
            query = query.AsNoTracking();

        // OrderBy on AppUser (EF Core translates this correctly)
        query = query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName);

        // Pagination before projection (more SQL-efficient)
        if (options.Skip.HasValue)
            query = query.Skip(options.Skip.Value);

        if (options.Take.HasValue)
            query = query.Take(options.Take.Value);

        // Project to UserSearchResult after ordering/pagination
        var result = await query
            .Select(u => new UserSearchResult(
                u.Id,
                u.UserName ?? string.Empty,
                u.FirstName,
                u.LastName,
                u.Email ?? string.Empty,
                u.ProfilePicturePath
            ))
            .ToListAsync(cancellationToken);

        // Apply filter after projection if provided (e.g. for UserSearchResult-specific filters)
        if (options.Filter is not null)
            result = result.Where(options.Filter.Compile()).ToList();

        return result;
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
}
