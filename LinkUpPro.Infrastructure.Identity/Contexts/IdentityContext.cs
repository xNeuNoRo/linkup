using LinkUpPro.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Identity.Contexts;

public class IdentityContext : IdentityDbContext<AppUser, IdentityRole<string>, string>
{
    public IdentityContext(DbContextOptions<IdentityContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuración de tablas y propiedades
        builder.HasDefaultSchema("Identity");

        # region User Configuration
        builder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity
                .Property(u => u.ProfilePicturePath)
                .IsRequired()
                .HasMaxLength(500)
                .HasDefaultValue("/images/default-avatar.png");
            entity.Property(u => u.IsActive).IsRequired().HasDefaultValue(false);
            entity.Property(u => u.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            entity.Property(u => u.UpdatedAt).IsRequired(false);
            entity.Property(u => u.DeletedAt).IsRequired(false);
            entity.Property(u => u.LastActivityAt).IsRequired(false);
            entity.Property(u => u.LastActivationEmailSentAt).IsRequired(false);

            entity.HasQueryFilter(u => u.DeletedAt == null);
        });
        # endregion

        # region Role Configuration
        builder.Entity<IdentityRole<string>>(entity =>
        {
            entity.ToTable("Roles");
        });

        builder.Entity<IdentityUserRole<string>>(entity =>
        {
            entity.ToTable("UserRoles");
        });
        # endregion

        # region Claims and Logins Configuration
        builder.Entity<IdentityUserClaim<string>>(entity =>
        {
            entity.ToTable("UserClaims");
        });

        builder.Entity<IdentityUserLogin<string>>(entity =>
        {
            entity.ToTable("UserLogins");
        });

        builder.Entity<IdentityUserToken<string>>(entity =>
        {
            entity.ToTable("UserTokens");
        });

        builder.Entity<IdentityRoleClaim<string>>(entity =>
        {
            entity.ToTable("RoleClaims");
        });
        # endregion
    }
}
