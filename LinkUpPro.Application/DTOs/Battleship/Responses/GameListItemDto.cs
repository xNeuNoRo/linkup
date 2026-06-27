namespace LinkUpPro.Application.DTOs.Battleship.Responses;

public record GameListItemDto(
    long Id,
    string OpponentId,
    string OpponentName,
    int Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    string? WinnerId,
    string? CurrentTurnUserId,
    TimeSpan? Duration
);
