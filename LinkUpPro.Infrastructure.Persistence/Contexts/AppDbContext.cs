using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Battleship;
using LinkUpPro.Domain.Entities.Friendship;
using LinkUpPro.Domain.Entities.Identity;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Infrastructure.Persistence.Providers;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Contexts;

/// <summary>
/// Contexto principal de Entity Framework Core para la persistencia del dominio.
/// Incluye DbSet para todas las entidades del dominio.
/// </summary>
public sealed class AppDbContext : DbContext
{
    private readonly IDateTimeProvider _dateTimeProvider;

    /// <summary>
    /// Constructor utilizado por las migraciones de EF Core (sin dependency injection).
    /// Recurre a una instancia por defecto de DateTimeProvider.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : this(options, new DateTimeProvider()) { }

    /// <summary>
    /// Constructor principal utilizado en runtime mediante dependency injection.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options, IDateTimeProvider dateTimeProvider)
        : base(options)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    // Identity / Social
    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // Friendship
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();

    // Battleship
    public DbSet<BattleshipGame> BattleshipGames => Set<BattleshipGame>();
    public DbSet<BattleshipShip> BattleshipShips => Set<BattleshipShip>();
    public DbSet<BattleshipAttack> BattleshipAttacks => Set<BattleshipAttack>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automáticamente todas las configuraciones IEntityTypeConfiguration<T>
        // definidas en este ensamblado.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        var now = _dateTimeProvider.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChanges();
    }
}
