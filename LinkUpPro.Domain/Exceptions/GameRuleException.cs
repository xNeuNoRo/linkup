namespace LinkUpPro.Domain.Exceptions;

/// <summary>
/// Exception raised when a Battleship rule is violated.
/// </summary>
public sealed class GameRuleException : DomainException
{
    public GameRuleException(string message, string code)
        : base(message, code)
    {
    }

    public GameRuleException(
        string message,
        string code,
        IReadOnlyDictionary<string, object?> metadata)
        : base(message, code, metadata)
    {
    }
}
