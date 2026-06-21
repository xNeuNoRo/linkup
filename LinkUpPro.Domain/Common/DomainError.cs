namespace LinkUpPro.Domain.Common;

/// <summary>
/// Representa un error de dominio con un código y un mensaje para el usuario.
/// </summary>
/// <param name="Code">Código de error</param>
/// <param name="Message">Mensaje de error para el usuario</param>
public sealed record DomainError(string Code, string Message)
{
    public static readonly DomainError None = new(string.Empty, string.Empty);
}
