using LinkUpPro.Domain.Settings;

namespace LinkUpPro.Tests.Domain.Settings;

public class MailSettingsTests
{
    [Fact]
    public void Defaults_UseExpectedSectionAndPort()
    {
        // Arrange & Act
        var settings = new MailSettings();

        // Assert
        Assert.Equal("MailSettings", MailSettings.SectionName);
        Assert.Equal(587, settings.SmtpPort);
        Assert.True(settings.UseSsl);
        Assert.Equal("LinkUp Pro System", settings.DisplayName);
    }

    [Fact]
    public void IsConfigured_WithCompleteSettings_ReturnsTrue()
    {
        // Arrange
        var settings = new MailSettings
        {
            EmailFrom = "noreply@linkuppro.com",
            SmtpHost = "smtp.example.com",
            SmtpPort = 587,
            SmtpUser = "smtp-user",
            SmtpPass = "smtp-pass",
        };

        // Act
        var isConfigured = settings.IsConfigured();

        // Assert
        Assert.True(isConfigured);
    }

    [Fact]
    public void IsConfigured_WithMissingSecret_ReturnsFalse()
    {
        // Arrange
        var settings = new MailSettings
        {
            EmailFrom = "noreply@linkuppro.com",
            SmtpHost = "smtp.example.com",
            SmtpPort = 587,
            SmtpUser = "smtp-user",
            SmtpPass = " ",
        };

        // Act
        var isConfigured = settings.IsConfigured();

        // Assert
        Assert.False(isConfigured);
    }
}
