using LinkUpPro.Domain.Entities.Friendship;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations;

public sealed class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.ToTable("Friendships");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.User1Id).IsRequired().HasMaxLength(32);
        builder.Property(x => x.User2Id).IsRequired().HasMaxLength(32);

        // Auditoría
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);
        builder.Property(x => x.DeletedAt).IsRequired(false);

        // Índices
        builder
            .HasIndex(x => new { x.User1Id, x.User2Id })
            .IsUnique()
            .HasDatabaseName("UX_Friendships_User1_User2")
            .HasFilter("[DeletedAt] IS NULL");

        builder.HasIndex(x => x.User1Id);
        builder.HasIndex(x => x.User2Id);

        // QueryFilter para evitar incluir amistades eliminadas en consultas normales
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
