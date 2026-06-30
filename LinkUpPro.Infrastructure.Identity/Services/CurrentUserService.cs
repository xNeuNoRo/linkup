using System.Security.Claims;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Common;
using LinkUpPro.Infrastructure.Identity.Contexts;
using LinkUpPro.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Identity.Services;

/// <summary>
/// Implementación de ICurrentUserService que extrae la identidad del usuario
/// desde el HttpContext actual (cookie de autenticación de ASP.NET Core Identity).
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<AppUser> _userManager;
    private readonly IdentityContext _identityContext;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<AppUser> userManager,
        IdentityContext identityContext
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _identityContext = identityContext;
    }

    private ClaimsPrincipal? Principal =>
        _httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName =>
        Principal?.FindFirstValue(ClaimTypes.Name) ?? Principal?.Identity?.Name;

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);

    public string? FullName
    {
        get
        {
            var firstName = Principal?.FindFirstValue("FirstName");
            var lastName = Principal?.FindFirstValue("LastName");
            if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
            {
                return $"{firstName} {lastName}".Trim();
            }
            return null;
        }
    }

    public string? ProfilePicturePath => Principal?.FindFirstValue("ProfilePicturePath");

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(UserId);

    public bool RememberMe =>
        bool.TryParse(Principal?.FindFirstValue("RememberMe"), out var rm) && rm;

    public DateTimeOffset? LastActivityAt
    {
        get
        {
            var value = Principal?.FindFirstValue("LastActivityAt");
            return DateTimeOffset.TryParse(value, out var dt) ? dt : null;
        }
    }

    public ClaimsPrincipal? GetPrincipal() => Principal;

    public async Task<bool> IsActiveAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated || string.IsNullOrEmpty(UserId))
            return false;

        var user = await _identityContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == UserId, cancellationToken);

        return user?.IsActive ?? false;
    }

    public async Task TouchLastActivityAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated || string.IsNullOrEmpty(UserId))
            return;

        await _identityContext.Users
            .Where(u => u.Id == UserId)
            .ExecuteUpdateAsync(
                u => u.SetProperty(x => x.LastActivityAt, DateTimeOffset.UtcNow),
                cancellationToken
            );
    }
}
