namespace LinkUpPro.Domain.Enums;

/// <summary>
/// Enum que representa el estado de una solicitud de amistad entre dos usuarios.
/// </summary>
public enum FriendRequestStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3,
    Canceled = 4,
}
