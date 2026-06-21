using System.Text.RegularExpressions;
using LinkUpPro.Domain.Exceptions;

namespace LinkUpPro.Domain.ValueObjects;

/// <summary>
/// Represents a validated YouTube video identifier extracted from supported YouTube URLs.
/// </summary>
public sealed record YouTubeVideoId
{
    private static readonly Regex VideoIdRegex = new(
        @"^[A-Za-z0-9_-]{11}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private YouTubeVideoId(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public string EmbedUrl => $"https://www.youtube.com/embed/{Value}";

    public string WatchUrl => $"https://www.youtube.com/watch?v={Value}";

    public static YouTubeVideoId Create(string urlOrId)
    {
        if (string.IsNullOrWhiteSpace(urlOrId))
        {
            throw new DomainException(
                "Debe ingresar un enlace valido de YouTube.",
                "Post.YouTubeUrlRequired");
        }

        var candidate = ExtractVideoId(urlOrId.Trim());

        if (!VideoIdRegex.IsMatch(candidate))
        {
            throw new DomainException(
                "Debe ingresar un enlace valido de YouTube.",
                "Post.InvalidYouTubeUrl");
        }

        return new YouTubeVideoId(candidate);
    }

    public override string ToString() => Value;

    private static string ExtractVideoId(string value)
    {
        if (VideoIdRegex.IsMatch(value))
        {
            return value;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return value;
        }

        var host = uri.Host.ToLowerInvariant();

        if (host is "youtu.be" or "www.youtu.be")
        {
            return uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        }

        if (!host.EndsWith("youtube.com", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length >= 2 && segments[0] is "embed" or "shorts")
        {
            return segments[1];
        }

        if (uri.AbsolutePath.Equals("/watch", StringComparison.OrdinalIgnoreCase))
        {
            return GetQueryValue(uri.Query, "v");
        }

        return string.Empty;
    }

    private static string GetQueryValue(string query, string key)
    {
        var trimmedQuery = query.TrimStart('?');
        var pairs = trimmedQuery.Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);

            if (parts.Length == 2 && parts[0].Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        return string.Empty;
    }
}
