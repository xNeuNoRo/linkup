using LinkUpPro.Domain.Common;

namespace LinkUpPro.Domain.Settings;

public sealed class FileSettings
{
    public const string SectionName = "FileSettings";

    public string BasePath { get; init; } = null!;

    public string UrlPrefix { get; init; } = "/uploads";

    public long MaxImageFileSizeBytes { get; init; } = DomainConstants.MaxImageFileSizeBytes;

    public IReadOnlySet<string> AllowedImageExtensions { get; init; } =
        DomainConstants.AllowedImageExtensions;

    public string[] AllowedMimeTypes { get; init; } = { "image/jpeg", "image/png", "image/webp" };

    public string UploadRootPath { get; init; } = "uploads";

    public string ProfilePicturesPath { get; init; } = "profiles";

    public string PostImagesPath { get; init; } = "posts";

    public bool IsAllowedImageExtension(string? extensionOrFileName)
    {
        if (string.IsNullOrWhiteSpace(extensionOrFileName))
            return false;

        var extension = NormalizeExtension(extensionOrFileName);
        return AllowedImageExtensions.Contains(extension);
    }

    public bool IsAllowedImageSize(long fileSizeBytes) =>
        fileSizeBytes > 0 && fileSizeBytes <= MaxImageFileSizeBytes;

    private static string NormalizeExtension(string extensionOrFileName)
    {
        var trimmed = extensionOrFileName.Trim();
        var extension = Path.GetExtension(trimmed);

        if (string.IsNullOrWhiteSpace(extension))
            extension = trimmed.StartsWith(".", StringComparison.Ordinal) ? trimmed : $".{trimmed}";

        return extension.ToLowerInvariant();
    }
}
