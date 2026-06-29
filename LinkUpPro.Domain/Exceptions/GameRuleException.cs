namespace LinkUpPro.Domain.Exceptions;

/// <summary>
/// Excepción que se lanza cuando una regla de juego es violada
/// durante la ejecución de un comando relacionado con el juego de Battleship.
/// </summary>
public sealed class GameRuleException : DomainException
{
    public GameRuleException(string message, string code)
        : base(message, code) { }

    public GameRuleException(
        string message,
        string code,
        IReadOnlyDictionary<string, object?> metadata
    )
        : base(message, code, metadata) { }
}
