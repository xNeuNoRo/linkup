namespace LinkUpPro.Domain.Exceptions;

/// <summary>
/// Exception raised when a concurrent operation invalidates the current domain command.
/// </summary>
public sealed class ConcurrencyException : DomainException
{
    public ConcurrencyException(string message, string code)
        : base(message, code)
    {
    }

    public ConcurrencyException(
        string message,
        string code,
        IReadOnlyDictionary<string, object?> metadata)
        : base(message, code, metadata)
    {
    }
}
