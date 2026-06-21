using LinkUpPro.Domain.Common;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using LinkUpPro.Infrastructure.Persistence.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LinkUpPro.Infrastructure.Persistence.Services;

/// <summary>
/// Inicializa la base de datos al arrancar la aplicación con datos de prueba
/// </summary>
public sealed class DatabaseInitializerHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _env;

    public DatabaseInitializerHostedService(IServiceScopeFactory scopeFactory, IHostEnvironment env)
    {
        _scopeFactory = scopeFactory;
        _env = env;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_env.IsDevelopment())
            return;

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        await DbSeeder.SeedAsync(context, configuration, dateTimeProvider, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
