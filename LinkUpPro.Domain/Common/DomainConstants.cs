using System.Collections.ObjectModel;

namespace LinkUpPro.Domain.Common;

/// <summary>
/// Constantes centralizadas de todas las reglas de negocio
/// </summary>
public static class DomainConstants
{
    public const int MaxUserNameLength = 100;
    public const int MaxUserFirstNameLength = 100;
    public const int MaxUserLastNameLength = 100;
    public const int MaxUserEmailLength = 256;
    public const int MaxProfilePicturePathLength = 500;

    public const int MaxPostContentLength = 1_000;
    public const int MaxCommentContentLength = 500;

    public const int BoardSize = 12;
    public const int BattleshipFleetShipCount = 5;
    public const int BattleshipRequiredHitCountToWin = 17;

    public const int MaxFailedAccessAttempts = 5;

    public static readonly TimeSpan LoginLockoutDuration = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan SessionInactivityTimeout = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan PersistentSessionDuration = TimeSpan.FromDays(7);
    public static readonly TimeSpan ActivationTokenLifetime = TimeSpan.FromHours(24);
    public static readonly TimeSpan PasswordResetTokenLifetime = TimeSpan.FromHours(1);
    public static readonly TimeSpan ActivationResendCooldown = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan BattleshipTurnTimeout = TimeSpan.FromHours(48);

    public const long MaxImageFileSizeBytes = 5 * 1024 * 1024;

    public static readonly IReadOnlySet<string> AllowedImageExtensions = new ReadOnlySet<string>(
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" }
    );

    public static readonly IReadOnlyList<int> RequiredBattleshipFleetSizes = Array.AsReadOnly([
        5,
        4,
        3,
        3,
        2,
    ]);
}
