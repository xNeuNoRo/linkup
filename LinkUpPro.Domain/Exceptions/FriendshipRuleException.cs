namespace LinkUpPro.Domain.Exceptions;

/// <summary>
/// Exception raised when a friendship or friend-request rule is violated.
/// </summary>
public sealed class FriendshipRuleException : DomainException
{
    public FriendshipRuleException(string message, string code)
        : base(message, code)
    {
    }

    public FriendshipRuleException(
        string message,
        string code,
        IReadOnlyDictionary<string, object?> metadata)
        : base(message, code, metadata)
    {
    }
}
