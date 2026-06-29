namespace LinkUpPro.Application.Models.Emails;

public record PasswordChangedModel(
    string UserName,
    string LoginUrl,
    DateTime ChangedAt,
    string SupportEmail = "soporte@linkuppro.com"
) : IEmailModel;
