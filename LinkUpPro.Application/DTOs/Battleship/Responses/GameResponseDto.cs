using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.DTOs.Battleship.Responses;

public record GameResponseDto(
    long Id, string CreatorId, string OpponentId, GameStatus Status,
    string? CurrentTurnUserId, string? WinnerId,
    DateTimeOffset StartedAt, DateTimeOffset? FinishedAt);
