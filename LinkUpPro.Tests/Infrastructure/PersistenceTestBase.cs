using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Tests.Infrastructure;

/// <summary>
/// Clase base para pruebas de integración con InMemory.
/// Proporciona métodos factory para crear AppDbContext con
/// un IDateTimeProvider fijo y datos de semilla reutilizables.
/// </summary>
public abstract class PersistenceTestBase
{
    private static int _databaseCounter;

    /// <summary>
    /// Crea un nuevo AppDbContext con InMemory.
    /// El nombre de la base de datos es único por invocación.
    /// </summary>
    protected static AppDbContext CreateContext()
    {
        var dbName = $"LinkUpProTestDb_{DateTimeOffset.UtcNow.Ticks}_{++_databaseCounter}";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var context = new AppDbContext(options, new TestDateTimeProvider());
        context.Database.EnsureCreated();
        return context;
    }

    protected static string CreateUserId(string prefix = "user")
    {
        return $"{prefix}_{Guid.NewGuid():N}"[..32];
    }
}
