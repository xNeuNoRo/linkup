namespace LinkUpPro.Application.DTOs.Battleship.Responses;

public record AttackResultDto(
    bool IsHit,
    bool IsSunk,
    bool IsGameOver,
    string? WinnerId,
    bool TurnChanged
);
