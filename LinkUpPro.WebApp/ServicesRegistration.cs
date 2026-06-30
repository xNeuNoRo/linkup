using Microsoft.Extensions.DependencyInjection;

namespace LinkUpPro.WebApp;

/// <summary>
/// Registro de servicios específicos de la capa de presentación (WebApp).
/// Aquí se registran middlewares, filtros y servicios relacionados con MVC/Razor.
/// </summary>
public static class ServicesRegistration
{
    public static IServiceCollection AddWebAppServices(this IServiceCollection services)
    {
        // Filtros de autorización personalizados
        services.AddScoped<Filters.SessionAuthorizeAttribute>();

        return services;
    }
}
