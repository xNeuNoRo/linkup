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
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Message).IsRequired().HasMaxLength(500);
        builder.Property(x => x.RelatedEntityId).IsRequired(false);
        builder.Property(x => x.RelatedEntityType).IsRequired();
        builder.Property(x => x.IsRead).IsRequired().HasDefaultValue(false);

        // Auditoría
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);
        builder.Property(x => x.DeletedAt).IsRequired(false);

        // QueryFilter para excluir notificaciones eliminadas de consultas normales
        builder.HasQueryFilter(x => x.DeletedAt == null);

        // Índices
        builder.HasIndex(x => x.RecipientId);
        builder
            .HasIndex(x => new { x.RecipientId, x.IsRead })
            .HasDatabaseName("IX_Notifications_Recipient_Unread");
        builder
            .HasIndex(x => new { x.RelatedEntityType, x.RelatedEntityId })
            .HasDatabaseName("IX_Notifications_RelatedEntity");
    }
}
