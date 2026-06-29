namespace LinkUpPro.Application.Models.Emails;

public record ActivationEmailModel(
    string UserName,
    string ActivationUrl,
    int ExpiryHours = 24,
    string SupportEmail = "soporte@linkuppro.com"
) : IEmailModel;
