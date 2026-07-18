using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Settings;

namespace LinkUpPro.Tests.Domain.Settings;

public class FileSettingsTests
{
    [Fact]
    public void Defaults_MatchFunctionalDocumentImageRules()
    {
        // Arrange & Act
        var settings = new FileSettings();

        // Assert
        Assert.Equal(DomainConstants.MaxImageFileSizeBytes, settings.MaxImageFileSizeBytes);
        Assert.Contains(".jpg", settings.AllowedImageExtensions);
        Assert.Contains(".jpeg", settings.AllowedImageExtensions);
        Assert.Contains(".png", settings.AllowedImageExtensions);
        Assert.Contains(".webp", settings.AllowedImageExtensions);
    }

    [Theory]
    [InlineData(".jpg")]
    [InlineData("jpeg")]
    [InlineData("profile.PNG")]
    [InlineData("photo.webp")]
    public void IsAllowedImageExtension_AllowedExtension_ReturnsTrue(string extensionOrFileName)
    {
        // Arrange
        var settings = new FileSettings();

        // Act
        var isAllowed = settings.IsAllowedImageExtension(extensionOrFileName);

        // Assert
        Assert.True(isAllowed);
    }

    [Theory]
    [InlineData(".gif")]
    [InlineData("script.js")]
    [InlineData("")]
    [InlineData(" ")]
    public void IsAllowedImageExtension_DisallowedExtension_ReturnsFalse(string extensionOrFileName)
    {
        // Arrange
        var settings = new FileSettings();

        // Act
        var isAllowed = settings.IsAllowedImageExtension(extensionOrFileName);

        // Assert
        Assert.False(isAllowed);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(DomainConstants.MaxImageFileSizeBytes, true)]
    [InlineData(0, false)]
    [InlineData(DomainConstants.MaxImageFileSizeBytes + 1, false)]
    public void IsAllowedImageSize_ReturnsExpectedValue(long fileSizeBytes, bool expected)
    {
        // Arrange
        var settings = new FileSettings();

        // Act
        var isAllowed = settings.IsAllowedImageSize(fileSizeBytes);

        // Assert
        Assert.Equal(expected, isAllowed);
    }
}
