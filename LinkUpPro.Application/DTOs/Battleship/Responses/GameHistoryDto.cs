namespace LinkUpPro.Application.DTOs.Battleship.Responses;

public record GameHistoryDto(
    long GameId, string OpponentId, string OpponentName,
    DateTimeOffset StartedAt, DateTimeOffset FinishedAt, TimeSpan Duration,
    bool IsWon, string Winner, string? OpponentProfilePicture);
