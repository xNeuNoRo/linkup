using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

/// <summary>
/// Valida que el texto no sea nulo, vacío ni contenga únicamente espacios en blanco.
/// Se usa para campos como Nombre, Apellido, Contenido de publicación/comentario.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NoWhitespaceOnlyAttribute : ValidationAttribute
{
    public NoWhitespaceOnlyAttribute()
        : base("El campo {0} no puede contener únicamente espacios en blanco.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is string s) return !string.IsNullOrWhiteSpace(s);
        return true;
    }
}
