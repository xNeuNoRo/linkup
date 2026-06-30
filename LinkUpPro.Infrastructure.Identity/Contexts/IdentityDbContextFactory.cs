using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LinkUpPro.Infrastructure.Identity.Contexts;

/// <summary>
/// Factory para crear instancias de IdentityContext en comandos CLI.
/// </summary>
public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityContext>
{
    public IdentityContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Establece el directorio base para buscar appsettings.json
            .AddJsonFile("appsettings.json", optional: false) // Carga la configuración desde appsettings.json
            .AddJsonFile( // Carga la configuración desde appsettings.Development.json si existe
                "appsettings.Development.json",
                optional: true
            )
            .AddJsonFile( // Carga la configuración desde appsettings.{Environment}.json si existe
                $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                optional: true
            )
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("LinkUpDb")
            ?? throw new InvalidOperationException(
                "La connection string 'LinkUpDb' no se encontro en la configuracion. "
                    + "Verifique que exista en appsettings.json o como variable de entorno "
            );

        var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
        optionsBuilder.UseSqlServer(
            connectionString,
            sqlOptions => sqlOptions.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)
        );

        return new IdentityContext(optionsBuilder.Options);
    }
}
