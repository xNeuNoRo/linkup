using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

/// <summary>
/// Valida que el archivo subido no exceda el tamaño máximo permitido (en MB).
/// Por defecto, 5 MB según los requisitos del documento funcional.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class MaxFileSizeAttribute : ValidationAttribute
{
    public int MaxSizeInMB { get; }

    public MaxFileSizeAttribute(int maxSizeInMB = 5)
        : base($"La imagen seleccionada no puede superar los {maxSizeInMB} MB.")
    {
        MaxSizeInMB = maxSizeInMB;
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is IFormFile file)
        {
            long maxBytes = (long)MaxSizeInMB * 1024 * 1024;
            return file.Length <= maxBytes;
        }
        return true;
    }
}
