using LinkUpPro.Domain.Entities.Battleship;
using LinkUpPro.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkUpPro.Infrastructure.Persistence.EntityConfigurations;

public sealed class BattleshipAttackConfiguration : IEntityTypeConfiguration<BattleshipAttack>
{
    public void Configure(EntityTypeBuilder<BattleshipAttack> builder)
    {
        builder.ToTable("BattleshipAttacks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.GameId).IsRequired();
        builder.Property(x => x.AttackerId).IsRequired().HasMaxLength(32);
        builder.Property(x => x.TargetX).IsRequired();
        builder.Property(x => x.TargetY).IsRequired();
        builder.Property(x => x.IsHit).IsRequired();
        builder.Property(x => x.TargetShipId).IsRequired(false);
        builder.Property(x => x.AttackDate).IsRequired();

        // Auditoría
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired(false);

        // Indices
        builder
            .HasIndex(x => new
            {
                x.GameId,
                x.AttackerId,
                x.TargetX,
                x.TargetY,
            })
            .IsUnique()
            .HasDatabaseName("UX_BattleshipAttacks_Game_Attacker_Coords");

        builder.HasIndex(x => x.GameId);
        builder
            .HasIndex(x => new { x.GameId, x.AttackerId })
            .HasDatabaseName("IX_BattleshipAttacks_Game_Attacker");

        // Relaciones
        builder
            .HasOne<BattleshipGame>()
            .WithMany()
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.AttackerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<BattleshipShip>()
            .WithMany()
            .HasForeignKey(x => x.TargetShipId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);
    }
}
