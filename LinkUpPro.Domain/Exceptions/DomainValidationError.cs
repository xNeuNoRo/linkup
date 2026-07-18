namespace LinkUpPro.Domain.Exceptions;

/// <summary>
/// Representa un error de validación en el dominio.
/// </summary>
/// <param name="PropertyName">Nombre de la propiedad que falló la validación.</param>
/// <param name="Message">Mensaje de validación para el usuario.</param>
/// <param name="Code">Código de validación.</param>
public sealed record DomainValidationError(string PropertyName, string Message, string Code);
