using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

/// <summary>
/// Valida que el archivo subido tenga una extensión permitida (.jpg, .jpeg, .png, .webp).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ImageFileExtensionsAttribute : ValidationAttribute
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    public ImageFileExtensionsAttribute()
        : base("El archivo seleccionado no tiene un formato de imagen válido. Formatos permitidos: .jpg, .jpeg, .png, .webp.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is IFormFile file)
        {
            if (file.Length == 0) return true;
            var ext = Path.GetExtension(file.FileName);
            return AllowedExtensions.Contains(ext);
        }
        return true;
    }
}
