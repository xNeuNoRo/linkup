using LinkUpPro.Infrastructure.Persistence.Contexts;
using LinkUpPro.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Tests.Base;

/// <summary>
/// Clase base para tests de integracion de servicios con base de datos InMemory.
/// Proporciona un DbContext limpio por clase de test, con datos de semilla
/// y helpers para servicios Identity.
/// </summary>
public abstract class InMemoryTestBase : IAsyncLifetime
{
    private static int _databaseCounter;
    private readonly string _dbName;

    protected AppDbContext DbContext { get; private set; } = null!;

    protected InMemoryTestBase()
    {
        _dbName = $"LinkUpProTestDb_{DateTimeOffset.UtcNow.Ticks}_{++_databaseCounter}";
    }

    public virtual async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;

        DbContext = new AppDbContext(options, new TestDateTimeProvider());
        await DbContext.Database.EnsureCreatedAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.DisposeAsync();
    }
}
