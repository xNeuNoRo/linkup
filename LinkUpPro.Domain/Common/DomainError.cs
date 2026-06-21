namespace LinkUpPro.Domain.Common;

/// <summary>
/// Represents a domain error with a stable code and a user-safe message.
/// </summary>
/// <param name="Code">Stable error code.</param>
/// <param name="Message">User-safe error message.</param>
public sealed record DomainError(string Code, string Message)
{
    public static readonly DomainError None = new(string.Empty, string.Empty);
}
