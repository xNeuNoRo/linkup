using LinkUpPro.Domain.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.RecipientId).IsRequired().HasMaxLength(32);
        builder.Property(x => x.ActorId).IsRequired().HasMaxLength(32);
        builder.Property(x => x.Type).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Message).IsRequired().HasMaxLength(500);
        builder.Property(x => x.RelatedPostId).IsRequired(false);
        builder.Property(x => x.IsRead).IsRequired().HasDefaultValue(false);

        // Auditoría
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        // Índices
        builder.HasIndex(x => x.RecipientId);
        builder
            .HasIndex(x => new { x.RecipientId, x.IsRead })
            .HasDatabaseName("IX_Notifications_Recipient_Unread");

        // Relaciones
        builder
            .HasOne<Post>()
            .WithMany()
            .HasForeignKey(x => x.RelatedPostId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
