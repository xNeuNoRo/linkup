using LinkUpPro.Application.Interfaces;
using LinkUpPro.Domain.Settings;
using LinkUpPro.Infrastructure.Shared.Messaging;
using LinkUpPro.Infrastructure.Shared.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkUpPro.Infrastructure.Shared;

public static class ServicesRegistration
{
    public static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MailSettings>(configuration.GetSection(MailSettings.SectionName));
        services.Configure<FileSettings>(configuration.GetSection(FileSettings.SectionName));

        services.AddSingleton<IRazorRenderer, RazorRenderer>();
        services.AddScoped<IEmailService, MailKitEmailService>();
        services.AddScoped<IFileService, FileService>();

        return services;
    }
}
