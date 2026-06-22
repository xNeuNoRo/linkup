namespace LinkUpPro.Application.Models.Emails;

public record ProfileUpdatedModel(
    string UserName,
    List<string> ChangesSummary,
    string ProfileUrl,
    string SupportEmail = "soporte@linkuppro.com"
) : IEmailModel;
