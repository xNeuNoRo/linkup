using LinkUpPro.Domain.Entities.Identity;
using LinkUpPro.Domain.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations;

public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.AuthorId).IsRequired().HasMaxLength(32);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.ContentType).IsRequired();
        builder.Property(x => x.MediaPath).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Privacy).IsRequired();
        builder.Property(x => x.AllowComments).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.IsEdited).IsRequired().HasDefaultValue(false);

        // Auditoría
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);
        builder.Property(x => x.DeletedAt).IsRequired(false);

        // Índices
        builder.HasIndex(x => x.AuthorId);
        builder.HasIndex(x => x.CreatedAt);
        builder
            .HasIndex(x => new { x.AuthorId, x.DeletedAt })
            .HasDatabaseName("IX_Posts_Author_Active");

        // Relaciones
        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // QueryFilter para excluir posts eliminados de consultas normales
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
