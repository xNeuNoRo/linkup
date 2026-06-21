using LinkUpPro.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasMaxLength(32).IsRequired();

        builder.Property(x => x.UserName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NormalizedUserName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.NormalizedEmail).IsRequired().HasMaxLength(256);
        builder.Property(x => x.EmailConfirmed).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ProfilePicturePath).IsRequired().HasMaxLength(500);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.PhoneNumberConfirmed).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.LockoutEnabled).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.LockoutEnd).IsRequired(false);
        builder.Property(x => x.AccessFailedCount).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.LastActivityAt).IsRequired(false);

        // Auditoría
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);
        builder.Property(x => x.DeletedAt).IsRequired(false);

        // PhoneNumber como owned type
        // Owned type es una forma de modelar un valor complejo (Value Object) dentro de la misma tabla del User,
        // sin necesidad de una tabla separada. En este caso, PhoneNumber es un Value Object que tiene una sola propiedad Value.
        builder.OwnsOne(
            x => x.PhoneNumber,
            pn =>
            {
                pn.Property(p => p.Value)
                    .HasColumnName("PhoneNumber")
                    .HasMaxLength(20)
                    .IsRequired();
            }
        );

        // Índices
        builder.HasIndex(x => x.NormalizedUserName).IsUnique().HasFilter("[DeletedAt] IS NULL");

        builder.HasIndex(x => x.NormalizedEmail).IsUnique().HasFilter("[DeletedAt] IS NULL");

        builder.HasIndex(x => x.IsActive);

        // QueryFilter para excluir usuarios eliminados de consultas normales (soft delete)
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
