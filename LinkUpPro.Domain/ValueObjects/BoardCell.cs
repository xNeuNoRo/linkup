using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Domain.ValueObjects;

public sealed record BoardCell(Coordinates Coordinates, BoardCellState State)
{
    public bool IsAttackResult => State is BoardCellState.Hit or BoardCellState.Miss;
}
