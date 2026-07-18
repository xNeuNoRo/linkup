namespace LinkUpPro.Application.DTOs.Battleship.Responses;

public record GameResultDto(
    long GameId,
    string OpponentId,
    string OpponentName,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    double DurationHours,
    string Result,
    string Winner,
    AttackBoardDto MyAttackBoard,
    AttackBoardDto OpponentAttackBoard,
    PlacementBoardDto MyPlacementBoard
);
