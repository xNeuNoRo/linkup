namespace LinkUpPro.Application.DTOs.Battleship.Responses;

public enum CellState : byte
{
    Empty = 0,
    Miss = 1,
    Hit = 2,
    Sunk = 3,
}

public record AttackBoardDto(
    long GameId,
    string PlayerId,
    CellState[,] Grid,
    string? CurrentTurnUserId,
    bool IsMyTurn,
    bool IsGameOver,
    string? WinnerId
);
