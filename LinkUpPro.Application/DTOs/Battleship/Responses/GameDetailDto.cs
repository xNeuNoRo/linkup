using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.DTOs.Battleship.Responses;

public record GameDetailDto(
    long GameId,
    string OpponentId,
    string OpponentName,
    GameStatus Status,
    DateTimeOffset StartedAt,
    string? CurrentTurnUserId,
    bool IsMyTurn,
    bool IsGameOver,
    string? WinnerId,
    TimeSpan? TurnElapsedTime
);
