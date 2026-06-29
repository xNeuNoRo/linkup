using LinkUpPro.Application.Models.Emails;

namespace LinkUpPro.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync<T>(
        string to,
        string subject,
        string templateName,
        T model,
        CancellationToken cancellationToken = default
    ) where T : IEmailModel;
}
