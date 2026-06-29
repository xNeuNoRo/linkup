namespace LinkUpPro.Application.DTOs.Battleship.Requests;

public record PlaceShipRequest(int ShipSize, int StartX, int StartY, int Direction);
