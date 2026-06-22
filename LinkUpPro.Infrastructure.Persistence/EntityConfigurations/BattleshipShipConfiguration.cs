using LinkUpPro.Domain.Entities.Battleship;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations;

public sealed class BattleshipShipConfiguration : IEntityTypeConfiguration<BattleshipShip>
{
    public void Configure(EntityTypeBuilder<BattleshipShip> builder)
    {
        builder.ToTable("BattleshipShips");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.GameId).IsRequired();
        builder.Property(x => x.PlayerId).IsRequired().HasMaxLength(32);
        builder.Property(x => x.Size).IsRequired();
        builder.Property(x => x.StartX).IsRequired();
        builder.Property(x => x.StartY).IsRequired();
        builder.Property(x => x.Direction).IsRequired();
        builder.Property(x => x.IsSunk).IsRequired().HasDefaultValue(false);

        // Auditoría
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        // Indices
        builder.HasIndex(x => x.GameId);
        builder
            .HasIndex(x => new { x.GameId, x.PlayerId })
            .HasDatabaseName("IX_BattleshipShips_Game_Player");

        // Relaciones
        builder
            .HasOne<BattleshipGame>()
            .WithMany()
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
