using LinkUpPro.Domain.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.PostId).IsRequired();
        builder.Property(x => x.AuthorId).IsRequired().HasMaxLength(32);
        builder.Property(x => x.ParentCommentId).IsRequired(false);
        builder.Property(x => x.Content).IsRequired().HasMaxLength(500);
        builder.Property(x => x.IsEdited).IsRequired().HasDefaultValue(false);

        // Auditoría
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);
        builder.Property(x => x.DeletedAt).IsRequired(false);

        // Índices
        builder.HasIndex(x => x.PostId);
        builder.HasIndex(x => x.AuthorId);
        builder.HasIndex(x => x.ParentCommentId);

        // Relaciones
        builder
            .HasOne<Post>()
            .WithMany()
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<Comment>()
            .WithMany()
            .HasForeignKey(x => x.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // QueryFilter para evitar incluir comentarios eliminados en consultas normales
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
