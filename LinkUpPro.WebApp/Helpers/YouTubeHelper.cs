namespace LinkUpPro.WebApp.Helpers;

/// <summary>
/// Helpers para el manejo de URLs de YouTube.
/// </summary>
public static class YouTubeHelper
{
    /// <summary>
    /// Convierte una URL de YouTube (watch?v=, youtu.be/, shorts/) a su URL de embed.
    /// </summary>
    public static string ToEmbedUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        var videoId = ExtractVideoId(url);
        return string.IsNullOrEmpty(videoId)
            ? string.Empty
            : $"https://www.youtube-nocookie.com/embed/{videoId}";
    }

    /// <summary>
    /// Extrae el ID de video de 11 caracteres de una URL de YouTube.
    /// </summary>
    public static string? ExtractVideoId(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var patterns = new[]
        {
            @"^([a-zA-Z0-9_-]{11})$",
            @"(?:youtube\.com\/embed\/)([a-zA-Z0-9_-]{11})",
            @"(?:youtube\.com\/watch\?v=)([a-zA-Z0-9_-]{11})(?:[&#?].*)?",
            @"(?:youtu\.be\/)([a-zA-Z0-9_-]{11})(?:[&#?].*)?",
            @"(?:youtube\.com\/shorts\/)([a-zA-Z0-9_-]{11})",
            @"(?:m\.youtube\.com\/watch\?v=)([a-zA-Z0-9_-]{11})(?:[&#?].*)?",
            @"(?:youtube\.com\/v\/)([a-zA-Z0-9_-]{11})",
            @"(?:youtube-nocookie\.com\/embed\/)([a-zA-Z0-9_-]{11})",
        };

        foreach (var pattern in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(url, pattern);
            if (match.Success)
                return match.Groups[1].Value;
        }

        return null;
    }

    /// <summary>
    /// Obtiene la URL de la miniatura (thumbnail) de un video de YouTube.
    /// </summary>
    public static string GetThumbnailUrl(string? videoId)
    {
        return string.IsNullOrEmpty(videoId)
            ? string.Empty
            : $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg";
    }
}
