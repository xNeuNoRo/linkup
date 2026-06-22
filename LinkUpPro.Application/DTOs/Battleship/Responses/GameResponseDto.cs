namespace LinkUpPro.Application.DTOs.Battleship.Responses;

public record GameResponseDto(
    long Id, string CreatorId, string OpponentId, int Status,
    string? CurrentTurnUserId, string? WinnerId,
    DateTimeOffset StartedAt, DateTimeOffset? FinishedAt);
