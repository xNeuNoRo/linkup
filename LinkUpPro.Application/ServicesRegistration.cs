using FluentValidation;
using LinkUpPro.Application.Mappings;
using LinkUpPro.Application.Services;
using LinkUpPro.Application.Interfaces.Services;
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

        services.AddScoped<IPostService, PostService>();

        return services;
    }
}

public class ApplicationMarker { }
