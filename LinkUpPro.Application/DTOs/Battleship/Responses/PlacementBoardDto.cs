using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.DTOs.Battleship.Responses;

public record ShipPlacementDto(
    long Id,
    ShipSize Size,
    int StartX,
    int StartY,
    ShipDirection Direction,
    bool IsSunk,
    int[][] OccupiedCells
);

public record PlacementBoardDto(long GameId, string PlayerId, ShipPlacementDto[] Ships);
