using LinkUpPro.Domain.Entities.Friendship;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations;

public sealed class FriendRequestConfiguration : IEntityTypeConfiguration<FriendRequest>
{
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        builder.ToTable("FriendRequests");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.SenderId).IsRequired().HasMaxLength(32);
        builder.Property(x => x.ReceiverId).IsRequired().HasMaxLength(32);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.SentAt).IsRequired();
        builder.Property(x => x.RespondedAt).IsRequired(false);
        builder.Property(x => x.IsVisibleForSender).IsRequired().HasDefaultValue(true);

        // Auditoría
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);
        builder.Property(x => x.DeletedAt).IsRequired(false);

        // QueryFilter para excluir solicitudes eliminadas de consultas normales
        builder.HasQueryFilter(x => x.DeletedAt == null);

        // Indices
        builder.HasIndex(x => x.ReceiverId);
        builder.HasIndex(x => x.SenderId);
        builder
            .HasIndex(x => new { x.ReceiverId, x.Status })
            .HasDatabaseName("IX_FriendRequests_Receiver_Status");
        builder
            .HasIndex(x => new
            {
                x.SenderId,
                x.Status,
                x.IsVisibleForSender,
            })
            .HasDatabaseName("IX_FriendRequests_Sender_Status_Visible");

    }
}
