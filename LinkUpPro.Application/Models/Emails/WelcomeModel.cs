namespace LinkUpPro.Application.Models.Emails;

public record WelcomeModel(
    string UserName,
    string LoginUrl
) : IEmailModel;
