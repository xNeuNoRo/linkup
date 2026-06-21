namespace LinkUpPro.Domain.Exceptions;

/// <summary>
/// Excepción que se lanza cuando una regla de amistad entre dos usuarios
/// es violada, como intentar enviar una solicitud de amistad a un usuario.
/// </summary>
public sealed class FriendshipRuleException : DomainException
{
    public FriendshipRuleException(string message, string code)
        : base(message, code) { }

    public FriendshipRuleException(
        string message,
        string code,
        IReadOnlyDictionary<string, object?> metadata
    )
        : base(message, code, metadata) { }
}
