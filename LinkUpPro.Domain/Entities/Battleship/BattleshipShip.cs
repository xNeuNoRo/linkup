using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Domain.Entities.Battleship;

public sealed class BattleshipShip : BaseEntity<long>
{
    private BattleshipShip()
    {
    }

    public long GameId { get; private set; }

    public string PlayerId { get; private set; } = null!;

    public ShipSize Size { get; private set; }

    public byte StartX { get; private set; }

    public byte StartY { get; private set; }

    public ShipDirection Direction { get; private set; }

    public bool IsSunk { get; private set; }
    public static Result<BattleshipShip> Place(
        long gameId,
        string playerId,
        ShipPlacement placement,
        DateTimeOffset? createdAt = null)
    {
        ArgumentNullException.ThrowIfNull(placement);

        var errors = new List<DomainError>();

        if (gameId < 0)
        {
            errors.Add(new DomainError("Battleship.InvalidGame", "La partida seleccionada no es valida."));
        }

        if (string.IsNullOrWhiteSpace(playerId))
        {
            errors.Add(new DomainError("Battleship.PlayerRequired", "El jugador del barco es requerido."));
        }

        if (errors.Count > 0)
        {
            return Result<BattleshipShip>.Failure(errors);
        }

        return Result<BattleshipShip>.Success(new BattleshipShip
        {
            GameId = gameId,
            PlayerId = playerId.Trim(),
            Size = placement.Size,
            StartX = placement.Start.X,
            StartY = placement.Start.Y,
            Direction = placement.Direction,
            IsSunk = false,
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
        });
    }

    public IReadOnlyList<Coordinates> GetOccupiedCells() =>
        Coordinates.Create(StartX, StartY).GetCellsTowards(Direction, (int)Size);

    public bool Occupies(Coordinates coordinates) => GetOccupiedCells().Contains(coordinates);

    public bool RefreshSunkState(IEnumerable<BattleshipAttack> attacks, DateTimeOffset? updatedAt = null)
    {
        ArgumentNullException.ThrowIfNull(attacks);

        var hitCells = attacks
            .Where(attack => attack.IsHit)
            .Select(attack => attack.GetTarget())
            .ToHashSet();

        var isNowSunk = GetOccupiedCells().All(hitCells.Contains);

        if (isNowSunk && !IsSunk)
        {
            IsSunk = true;
            UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
        }

        return IsSunk;
    }
}
