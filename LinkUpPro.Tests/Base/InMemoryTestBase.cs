using LinkUpPro.Application.Mappings;
using LinkUpPro.Infrastructure.Identity.Contexts;
using LinkUpPro.Infrastructure.Identity.Entities;
using LinkUpPro.Infrastructure.Identity.Mappings;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using LinkUpPro.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkUpPro.Tests.Base;

/// <summary>
/// Clase base para tests de integracion de servicios con base de datos InMemory.
/// Proporciona un DbContext limpio por clase de test, con datos de semilla
/// y helpers para servicios Identity.
/// </summary>
public abstract class InMemoryTestBase : IAsyncLifetime
{
    private static int _databaseCounter;
    private static bool _mappingsRegistered;
    private static readonly object _lock = new();
    private readonly string _dbName;
    private readonly string _identityDbName;

    protected AppDbContext DbContext { get; private set; } = null!;

    protected IdentityContext IdentityDbContext { get; private set; } = null!;

    protected UserManager<AppUser> UserManager { get; private set; } = null!;

    protected InMemoryTestBase()
    {
        var suffix = $"{DateTimeOffset.UtcNow.Ticks}_{++_databaseCounter}";
        _dbName = $"LinkUpProTestDb_{suffix}";
        _identityDbName = $"LinkUpProIdentityTestDb_{suffix}";

        if (!_mappingsRegistered)
        {
            lock (_lock)
            {
                if (!_mappingsRegistered)
                {
                    MappingConfig.RegisterMappings();
                    ValueObjectMappingConfig.RegisterValueObjectMappings();
                    IdentityMappingConfig.RegisterMappings();
                    _mappingsRegistered = true;
                }
            }
        }
    }

    public virtual async Task InitializeAsync()
    {
        var appOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_dbName)
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        DbContext = new AppDbContext(appOptions, new TestDateTimeProvider());
        await DbContext.Database.EnsureCreatedAsync();

        var identityOptions = new DbContextOptionsBuilder<IdentityContext>()
            .UseInMemoryDatabase(_identityDbName)
            .Options;

        IdentityDbContext = new IdentityContext(identityOptions);
        await IdentityDbContext.Database.EnsureCreatedAsync();

        UserManager = BuildUserManager(IdentityDbContext);
    }

    public virtual async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.DisposeAsync();
        await IdentityDbContext.Database.EnsureDeletedAsync();
        await IdentityDbContext.DisposeAsync();
    }

    protected static UserManager<AppUser> BuildUserManager(IdentityContext context)
    {
        var userStore = new UserStore<AppUser, IdentityRole<string>, IdentityContext, string>(context);
        var identityOptions = Options.Create(new IdentityOptions
        {
            Password = new PasswordOptions
            {
                RequiredLength = 8,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = true,
            },
        });
        var passwordHasher = new PasswordHasher<AppUser>();
        var userValidators = new IUserValidator<AppUser>[] { new UserValidator<AppUser>() };
        var passwordValidators = new IPasswordValidator<AppUser>[] { new PasswordValidator<AppUser>() };
        var keyNormalizer = new UpperInvariantLookupNormalizer();
        var errors = new IdentityErrorDescriber();
        var services = new ServiceCollection().BuildServiceProvider();
        var logger = new Logger<UserManager<AppUser>>(new LoggerFactory());

        return new UserManager<AppUser>(
            userStore,
            identityOptions,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            services,
            logger);
    }
}
