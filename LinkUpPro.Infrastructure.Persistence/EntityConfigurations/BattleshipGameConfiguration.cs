using LinkUpPro.Domain.Entities.Battleship;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations;

public sealed class BattleshipGameConfiguration : IEntityTypeConfiguration<BattleshipGame>
{
    public void Configure(EntityTypeBuilder<BattleshipGame> builder)
    {
        builder.ToTable("BattleshipGames");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.CreatorId).IsRequired().HasMaxLength(32);
        builder.Property(x => x.OpponentId).IsRequired().HasMaxLength(32);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CurrentTurnUserId).IsRequired(false).HasMaxLength(32);
        builder.Property(x => x.WinnerId).IsRequired(false).HasMaxLength(32);
        builder.Property(x => x.StartedAt).IsRequired();
        builder.Property(x => x.TurnAssignedAt).IsRequired(false);
        builder.Property(x => x.FinishedAt).IsRequired(false);

        // Auditoría
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);
        builder.Property(x => x.DeletedAt).IsRequired(false);

        // Indices
        builder.HasIndex(x => x.CreatorId);
        builder.HasIndex(x => x.OpponentId);
        builder.HasIndex(x => x.Status).HasDatabaseName("IX_BattleshipGames_Status");

        // QueryFilter para evitar incluir partidas eliminadas en consultas normales
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
