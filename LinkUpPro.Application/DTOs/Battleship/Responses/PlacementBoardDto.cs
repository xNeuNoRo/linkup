namespace LinkUpPro.Application.DTOs.Battleship.Responses;

public record ShipPlacementDto(
    long Id,
    int Size,
    int StartX,
    int StartY,
    int Direction,
    bool IsSunk,
    int[][] OccupiedCells
);

public record PlacementBoardDto(long GameId, string PlayerId, ShipPlacementDto[] Ships);
