namespace LinkUpPro.Domain.Settings;

/// <summary>
/// SMTP settings used by the email infrastructure service.
/// </summary>
public sealed class MailSettings
{
    public const string SectionName = "MailSettings";

    public string EmailFrom { get; init; } = string.Empty;

    public string SmtpHost { get; init; } = string.Empty;

    public int SmtpPort { get; init; } = 587;

    public string SmtpUser { get; init; } = string.Empty;

    public string SmtpPass { get; init; } = string.Empty;

    public string DisplayName { get; init; } = "LinkUp Pro System";

    public bool UseSsl { get; init; } = true;

    public bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(EmailFrom) &&
        !string.IsNullOrWhiteSpace(SmtpHost) &&
        SmtpPort > 0 &&
        !string.IsNullOrWhiteSpace(SmtpUser) &&
        !string.IsNullOrWhiteSpace(SmtpPass);
}
