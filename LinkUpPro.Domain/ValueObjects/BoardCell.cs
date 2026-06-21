namespace LinkUpPro.Domain.ValueObjects;

/// <summary>
/// Representa el estado de una celda en el tablero de Battleship,
/// incluyendo sus coordenadas y si contiene un barco, un impacto o un fallo.
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
