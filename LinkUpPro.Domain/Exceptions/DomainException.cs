namespace LinkUpPro.Domain.Exceptions;

/// <summary>
/// Base exception for business-rule violations inside the domain model.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message, string code)
        : this(message, code, null, null)
    {
    }

    public DomainException(string message, string code, Exception? innerException)
        : this(message, code, null, innerException)
    {
    }

    public DomainException(
        string message,
        string code,
        IReadOnlyDictionary<string, object?>? metadata,
        Exception? innerException = null)
        : base(message, innerException)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("The exception code is required.", nameof(code));
        }

        Code = code;
        Metadata = metadata ?? new Dictionary<string, object?>();
    }

    public string Code { get; }

    public IReadOnlyDictionary<string, object?> Metadata { get; }
}
