using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.Exceptions;
using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Tests.Domain.ValueObjects;

public class ShipPlacementTests
{
    [Fact]
    public void Create_ValidPlacement_ReturnsPlacementWithOccupiedCells()
    {
        // Arrange
        var start = Coordinates.Create(2, 2);

        // Act
        var placement = ShipPlacement.Create(start, ShipDirection.Down, ShipSize.Size4);

        // Assert
        Assert.Equal(4, placement.Length);
        Assert.Equal([
            Coordinates.Create(2, 2),
            Coordinates.Create(2, 3),
            Coordinates.Create(2, 4),
            Coordinates.Create(2, 5),
        ], placement.GetOccupiedCells());
    }

    [Fact]
    public void Create_OutOfBoundsPlacement_ThrowsGameRuleException()
    {
        // Arrange
        var start = Coordinates.Create(10, 10);

        // Act
        void Act() => _ = ShipPlacement.Create(start, ShipDirection.Right, ShipSize.Size3);

        // Assert
        Assert.Throws<GameRuleException>(Act);
    }

    [Fact]
    public void OverlapsWith_OverlappingPlacement_ReturnsTrue()
    {
        // Arrange
        var existing = ShipPlacement.Create(Coordinates.Create(2, 2), ShipDirection.Right, ShipSize.Size3);
        var current = ShipPlacement.Create(Coordinates.Create(3, 2), ShipDirection.Down, ShipSize.Size3);

        // Act
        var overlaps = current.OverlapsWith([existing]);

        // Assert
        Assert.True(overlaps);
    }

    [Fact]
    public void OverlapsWith_NonOverlappingPlacement_ReturnsFalse()
    {
        // Arrange
        var existing = ShipPlacement.Create(Coordinates.Create(2, 2), ShipDirection.Right, ShipSize.Size3);
        var current = ShipPlacement.Create(Coordinates.Create(8, 8), ShipDirection.Down, ShipSize.Size3);

        // Act
        var overlaps = current.OverlapsWith([existing]);

        // Assert
        Assert.False(overlaps);
    }
}
