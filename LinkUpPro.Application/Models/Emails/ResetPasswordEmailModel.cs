namespace LinkUpPro.Application.Models.Emails;

public record ResetPasswordEmailModel(
    string UserName,
    string ResetUrl,
    int ExpiryHours = 1,
    string SupportEmail = "soporte@linkuppro.com"
) : IEmailModel;
