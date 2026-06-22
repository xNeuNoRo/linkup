using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LinkUpPro.Infrastructure.Identity.Contexts;

public class DesignTimeIdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
{
    public IdentityContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
        var connectionString = Environment.GetEnvironmentVariable("LINKUP_CONNECTION")
            ?? "Server=localhost\\SQLEXPRESS;Database=LinkUpProDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        optionsBuilder.UseSqlServer(connectionString,
            sqlOptions => sqlOptions.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName));

        return new IdentityContext(optionsBuilder.Options);
    }
}
