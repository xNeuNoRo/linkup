namespace LinkUpPro.Domain.Exceptions;

/// <summary>
/// Excepción que se lanza cuando una o más validaciones
/// de dominio fallan durante la ejecución de un comando o regla de negocio.
/// </summary>
public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(IEnumerable<DomainValidationError> validationErrors)
        : base(
            "One or more domain validation errors occurred.",
            "Domain.ValidationFailed",
            CreateMetadata(validationErrors)
        )
    {
        ValidationErrors = [.. validationErrors];
    }

    public DomainValidationException(string propertyName, string message, string code)
        : this([new DomainValidationError(propertyName, message, code)]) { }

    public IReadOnlyCollection<DomainValidationError> ValidationErrors { get; }

    private static IReadOnlyDictionary<string, object?> CreateMetadata(
        IEnumerable<DomainValidationError> validationErrors
    )
    {
        ArgumentNullException.ThrowIfNull(validationErrors);

        var errors = validationErrors.ToArray();

        if (errors.Length == 0)
        {
            throw new ArgumentException(
                "At least one validation error is required.",
                nameof(validationErrors)
            );
        }

        return new Dictionary<string, object?> { ["ValidationErrors"] = errors };
    }
}
