namespace LinkUpPro.Domain.Exceptions;

/// <summary>
/// Represents a single field-level domain validation error.
/// </summary>
/// <param name="PropertyName">Property or rule name that failed.</param>
/// <param name="Message">User-safe validation message.</param>
/// <param name="Code">Stable validation code.</param>
public sealed record DomainValidationError(string PropertyName, string Message, string Code);
