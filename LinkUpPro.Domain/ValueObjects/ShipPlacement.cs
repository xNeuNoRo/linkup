using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.Exceptions;

namespace LinkUpPro.Domain.ValueObjects;

/// <summary>
/// Representa la colocación de un barco en el tablero de Battleship, incluyendo su posición inicial,
/// dirección y tamaño, con métodos para obtener las celdas ocupadas y verificar solapamientos con otras colocaciones.
/// </summary>
public sealed record ShipPlacement(Coordinates Start, ShipDirection Direction, ShipSize Size)
{
    public int Length => (int)Size;

    public IReadOnlyList<Coordinates> GetOccupiedCells() =>
        Start.GetCellsTowards(Direction, Length);

    public static ShipPlacement Create(Coordinates start, ShipDirection direction, ShipSize size)
    {
        if (!Enum.IsDefined(size))
        {
            throw new GameRuleException(
                "El tamano del barco seleccionado no es valido.",
                "Battleship.InvalidShipSize"
            );
        }

        if (!Enum.IsDefined(direction))
        {
            throw new GameRuleException(
                "La direccion seleccionada no es valida.",
                "Battleship.InvalidShipDirection"
            );
        }

        var placement = new ShipPlacement(start, direction, size);

        _ = placement.GetOccupiedCells();

        return placement;
    }

    public bool OverlapsWith(IEnumerable<ShipPlacement> existingPlacements)
    {
        ArgumentNullException.ThrowIfNull(existingPlacements);

        var occupiedCells = GetOccupiedCells().ToHashSet();

        return existingPlacements
            .SelectMany(placement => placement.GetOccupiedCells())
            .Any(occupiedCells.Contains);
    }
}
