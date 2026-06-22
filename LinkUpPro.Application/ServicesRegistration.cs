using FluentValidation;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LinkUpPro.Application;

public static class ServicesRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton(config);

        services.AddValidatorsFromAssemblyContaining<ApplicationMarker>();

        return services;
    }
}

public class ApplicationMarker { }
