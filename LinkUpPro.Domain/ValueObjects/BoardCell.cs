namespace LinkUpPro.Domain.ValueObjects;

/// <summary>
/// Read model value object used to represent a single Battleship board cell state.
/// </summary>
public sealed record BoardCell(Coordinates Coordinates, BoardCellState State)
{
    public bool IsAttackResult => State is BoardCellState.Hit or BoardCellState.Miss;
}

public enum BoardCellState
{
    Empty = 0,
    Ship = 1,
    Hit = 2,
    Miss = 3,
}
