using LinkUpPro.Domain.Entities.Identity;
using LinkUpPro.Domain.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations;

public sealed class ReactionConfiguration : IEntityTypeConfiguration<Reaction>
{
    public void Configure(EntityTypeBuilder<Reaction> builder)
    {
        builder.ToTable("Reactions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.PostId).IsRequired();
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(32);
        builder.Property(x => x.Type).IsRequired();

        // Auditoría
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        // Índices
        builder
            .HasIndex(x => new { x.PostId, x.UserId })
            .IsUnique()
            .HasDatabaseName("UX_Reactions_Post_User");

        builder.HasIndex(x => x.PostId);
        builder.HasIndex(x => x.UserId);

        // Relaciones
        builder
            .HasOne<Post>()
            .WithMany()
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
