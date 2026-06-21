using LinkUpPro.Domain.Entities.Battleship;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Tests.Domain.Entities.Battleship;

public class BattleshipShipTests
{
    [Fact]
    public void Place_ValidPlacement_CreatesShip()
    {
        // Arrange
        var placement = ShipPlacement.Create(Coordinates.Create(0, 0), ShipDirection.Right, ShipSize.Size3);

        // Act
        var result = BattleshipShip.Place(1, "player", placement);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("player", result.Value.PlayerId);
        Assert.Equal(ShipSize.Size3, result.Value.Size);
        Assert.Equal(ShipDirection.Right, result.Value.Direction);
    }

    [Fact]
    public void GetOccupiedCells_RightDirection_ReturnsExpectedCells()
    {
        // Arrange
        var ship = BattleshipShip.Place(
            1,
            "player",
            ShipPlacement.Create(Coordinates.Create(0, 0), ShipDirection.Right, ShipSize.Size3)).Value;

        // Act
        var cells = ship.GetOccupiedCells();

        // Assert
        Assert.Equal([
            Coordinates.Create(0, 0),
            Coordinates.Create(1, 0),
            Coordinates.Create(2, 0),
        ], cells);
    }

    [Fact]
    public void RefreshSunkState_AllOccupiedCellsHit_MarksShipAsSunk()
    {
        // Arrange
        var ship = BattleshipShip.Place(
            1,
            "player",
            ShipPlacement.Create(Coordinates.Create(0, 0), ShipDirection.Right, ShipSize.Size2)).Value;

        var attacks = new[]
        {
            BattleshipAttack.Record(1, "opponent", Coordinates.Create(0, 0), isHit: true).Value,
            BattleshipAttack.Record(1, "opponent", Coordinates.Create(1, 0), isHit: true).Value,
        };

        // Act
        var isSunk = ship.RefreshSunkState(attacks);

        // Assert
        Assert.True(isSunk);
        Assert.True(ship.IsSunk);
    }
}
