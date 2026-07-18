using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.DTOs.Battleship.Responses;

public record AttackBoardDto(
    long GameId,
    string PlayerId,
    BoardCellState[,] Grid,
    string? CurrentTurnUserId,
    bool IsMyTurn,
    bool IsGameOver,
    string? WinnerId
);
