using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.Exceptions;

namespace LinkUpPro.Domain.ValueObjects;

/// <summary>
/// Immutable Battleship board coordinate. Valid values are from 0 to 11 on each axis.
/// </summary>
public readonly record struct Coordinates(byte X, byte Y)
{
    public static Coordinates Create(int x, int y)
    {
        if (!IsValid(x, y))
        {
            throw new GameRuleException(
                "La coordenada seleccionada esta fuera del tablero.",
                "Battleship.CoordinatesOutOfBounds",
                new Dictionary<string, object?>
                {
                    ["X"] = x,
                    ["Y"] = y,
                });
        }

        return new Coordinates((byte)x, (byte)y);
    }

    public static bool IsValid(int x, int y) =>
        x >= 0 && x < DomainConstants.BoardSize &&
        y >= 0 && y < DomainConstants.BoardSize;

    public IReadOnlyList<Coordinates> GetCellsTowards(ShipDirection direction, int size)
    {
        if (size <= 0)
        {
            throw new GameRuleException(
                "El tamano del barco debe ser mayor que cero.",
                "Battleship.InvalidShipSize");
        }

        var cells = new List<Coordinates>(size);

        for (var offset = 0; offset < size; offset++)
        {
            (int x, int y) = direction switch
            {
                ShipDirection.Up => ((int)X, (int)Y - offset),
                ShipDirection.Down => ((int)X, (int)Y + offset),
                ShipDirection.Left => ((int)X - offset, (int)Y),
                ShipDirection.Right => ((int)X + offset, (int)Y),
                _ => throw new GameRuleException(
                    "La direccion seleccionada no es valida.",
                    "Battleship.InvalidShipDirection"),
            };

            cells.Add(Create(x, y));
        }

        return cells;
    }

    public int DistanceTo(Coordinates other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
}
