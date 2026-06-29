using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Domain.Entities.Battleship;

public sealed class BattleshipAttack : AuditableBaseEntity<long>
{
    private BattleshipAttack() { }

    public long GameId { get; private set; }

    public string AttackerId { get; private set; } = null!;

    public byte TargetX { get; private set; }

    public byte TargetY { get; private set; }

    public bool IsHit { get; private set; }

    public long? TargetShipId { get; private set; }

    public DateTimeOffset AttackDate { get; private set; }

    public static Result<BattleshipAttack> Record(
        long gameId,
        string attackerId,
        Coordinates target,
        bool isHit,
        long? targetShipId = null,
        DateTimeOffset? attackDate = null
    )
    {
        var errors = new List<DomainError>();

        if (gameId < 0)
        {
            errors.Add(
                new DomainError("Battleship.InvalidGame", "La partida seleccionada no es valida.")
            );
        }

        if (string.IsNullOrWhiteSpace(attackerId))
        {
            errors.Add(new DomainError("Battleship.AttackerRequired", "El atacante es requerido."));
        }

        if (targetShipId <= 0)
        {
            targetShipId = null;
        }

        if (errors.Count > 0)
        {
            return Result<BattleshipAttack>.Failure(errors);
        }

        var date = attackDate ?? DateTimeOffset.UtcNow;

        return Result<BattleshipAttack>.Success(
            new BattleshipAttack
            {
                GameId = gameId,
                AttackerId = attackerId.Trim(),
                TargetX = target.X,
                TargetY = target.Y,
                IsHit = isHit,
                TargetShipId = targetShipId,
                AttackDate = date,
                CreatedAt = date,
            }
        );
    }

    public Coordinates GetTarget() => Coordinates.Create(TargetX, TargetY);
}
