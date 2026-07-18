using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

/// <summary>
/// Valida que el IFormFile sea válido (existe, no está vacío y tiene nombre).
/// Hereda de RequiredAttribute para integrarse con MVC model binding estándar
/// y jQuery unobtrusive validation en el cliente.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RequiredFileAttribute : RequiredAttribute
{
    public RequiredFileAttribute()
    {
        ErrorMessage = "Debe seleccionar un archivo.";
    }

    public override bool IsValid(object? value)
    {
        if (!base.IsValid(value)) return false;
        if (value is IFormFile file)
        {
            return file.Length > 0 && !string.IsNullOrWhiteSpace(file.FileName);
        }
        return false;
    }
}
