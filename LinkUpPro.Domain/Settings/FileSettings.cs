using LinkUpPro.Domain.Common;

namespace LinkUpPro.Domain.Settings;

/// <summary>
/// File upload settings used by shared infrastructure services.
/// </summary>
public sealed class FileSettings
{
    public const string SectionName = "FileSettings";

    public long MaxImageFileSizeBytes { get; init; } = DomainConstants.MaxImageFileSizeBytes;

    public IReadOnlySet<string> AllowedImageExtensions { get; init; } = DomainConstants.AllowedImageExtensions;

    public string UploadRootPath { get; init; } = "uploads";

    public string ProfilePicturesPath { get; init; } = "profiles";

    public string PostImagesPath { get; init; } = "posts";

    public bool IsAllowedImageExtension(string? extensionOrFileName)
    {
        if (string.IsNullOrWhiteSpace(extensionOrFileName))
        {
            return false;
        }

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
        {
            extension = trimmed.StartsWith(".", StringComparison.Ordinal) ? trimmed : $".{trimmed}";
        }

        return extension.ToLowerInvariant();
    }
}
