using FluentValidation;
using LinkUpPro.Application.Mappings;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace LinkUpPro.Application;

public static class ServicesRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        MappingConfig.RegisterMappings();

        var config = TypeAdapterConfig.GlobalSettings;
        services.AddSingleton(config);

        services.AddValidatorsFromAssemblyContaining<ApplicationMarker>();

        return services;
    }
}

public class ApplicationMarker { }
