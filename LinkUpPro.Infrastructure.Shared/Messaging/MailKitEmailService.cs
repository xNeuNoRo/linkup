using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Models.Emails;
using LinkUpPro.Domain.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LinkUpPro.Infrastructure.Shared.Messaging;

public class MailKitEmailService : IEmailService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MailKitEmailService> _logger;
    private readonly MailSettings _mailSettings;

    private static readonly SemaphoreSlim _smtpSemaphore = new(1, 1);

    public MailKitEmailService(
        IServiceScopeFactory scopeFactory,
        ILogger<MailKitEmailService> logger,
        IOptions<MailSettings> mailSettings
    )
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _mailSettings = mailSettings.Value;
    }

    public async Task<bool> SendEmailAsync<T>(
        string to,
        string subject,
        string templateName,
        T model,
        CancellationToken cancellationToken = default
    )
        where T : IEmailModel
    {
        bool acquired = await _smtpSemaphore.WaitAsync(TimeSpan.FromSeconds(30), cancellationToken);

        if (!acquired)
        {
            _logger.LogError(
                "No se pudo obtener turno en el semaforo SMTP. Tiempo de espera agotado para: {Email}",
                to
            );
            return false;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var renderer = scope.ServiceProvider.GetRequiredService<IRazorRenderer>();

            string sanitizedTemplateName = Path.GetFileNameWithoutExtension(templateName);
            string templatePath = Path.Combine(
                AppContext.BaseDirectory,
                "Templates",
                "Emails",
                $"{sanitizedTemplateName}.cshtml"
            );

            if (!File.Exists(templatePath))
            {
                _logger.LogError("Plantilla de correo no encontrada: {Path}", templatePath);
                return false;
            }

            string htmlBody = await renderer.RenderTemplateAsync(templatePath, model);

            var message = new MimeMessage();
            message.From.Add(
                new MailboxAddress(_mailSettings.DisplayName, _mailSettings.EmailFrom)
            );
            message.To.Add(new MailboxAddress(string.Empty, to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            var secureSocketOptions = _mailSettings.UseSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

            await client.ConnectAsync(
                _mailSettings.SmtpHost,
                _mailSettings.SmtpPort,
                secureSocketOptions,
                cancellationToken
            );

            if (!string.IsNullOrEmpty(_mailSettings.SmtpUser))
            {
                await client.AuthenticateAsync(
                    _mailSettings.SmtpUser,
                    _mailSettings.SmtpPass,
                    cancellationToken
                );
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation(
                "Correo enviado exitosamente a {Email}. Asunto: {Subject}",
                to,
                subject
            );

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error al enviar correo a {Email}", to);
            return false;
        }
        finally
        {
            _smtpSemaphore.Release();
        }
    }
}
