namespace LinkUpPro.Application.Models.Emails;

public record AccountActivatedModel(
    string UserName,
    string LoginUrl
) : IEmailModel;
