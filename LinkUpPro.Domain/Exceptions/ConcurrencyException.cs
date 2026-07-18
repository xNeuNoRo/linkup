namespace LinkUpPro.Domain.Exceptions;

/// <summary>
/// Excepción que se lanza cuando ocurre un conflicto de
/// concurrencia al intentar actualizar una entidad en la base de datos.
/// </summary>
public sealed class ConcurrencyException : DomainException
{
    public ConcurrencyException(string message, string code)
        : base(message, code) { }

    public ConcurrencyException(
        string message,
        string code,
        IReadOnlyDictionary<string, object?> metadata
    )
        : base(message, code, metadata) { }
}
