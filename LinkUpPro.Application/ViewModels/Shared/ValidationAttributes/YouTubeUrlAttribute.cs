using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

/// <summary>
/// Valida que la URL proporcionada corresponda a un video válido de YouTube.
/// Acepta formatos:
///   - https://www.youtube.com/watch?v=VIDEO_ID
///   - https://youtu.be/VIDEO_ID
///   - https://www.youtube.com/embed/VIDEO_ID
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class YouTubeUrlAttribute : ValidationAttribute
{
    public YouTubeUrlAttribute()
        : base("Debe ingresar un enlace válido de YouTube.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is not string s) return false;
        if (string.IsNullOrWhiteSpace(s)) return true;

        var patterns = new[]
        {
            @"^(https?://)?(www\.)?youtube\.com/watch\?v=[\w-]{11}",
            @"^(https?://)?youtu\.be/[\w-]{11}",
            @"^(https?://)?(www\.)?youtube\.com/embed/[\w-]{11}"
        };

        foreach (var p in patterns)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(s, p))
                return true;
        }
        return false;
    }
}
