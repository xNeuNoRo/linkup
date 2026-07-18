using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.Exceptions;
using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Tests.Domain.ValueObjects;

public class CoordinatesTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(11, 11)]
    [InlineData(5, 7)]
    public void Create_ValidCoordinates_ReturnsCoordinates(int x, int y)
    {
        // Arrange & Act
        var coordinates = Coordinates.Create(x, y);

        // Assert
        Assert.Equal((byte)x, coordinates.X);
        Assert.Equal((byte)y, coordinates.Y);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(12, 0)]
    [InlineData(0, 12)]
    public void Create_OutOfBoundsCoordinates_ThrowsGameRuleException(int x, int y)
    {
        // Arrange & Act
        void Act() => _ = Coordinates.Create(x, y);

        // Assert
        Assert.Throws<GameRuleException>(Act);
    }

    [Fact]
    public void GetCellsTowards_Right_ReturnsExpectedCells()
    {
        // Arrange
        var start = Coordinates.Create(2, 3);

        // Act
        var cells = start.GetCellsTowards(ShipDirection.Right, 3);

        // Assert
        Assert.Equal([
            Coordinates.Create(2, 3),
            Coordinates.Create(3, 3),
            Coordinates.Create(4, 3),
        ], cells);
    }

    [Fact]
    public void GetCellsTowards_UpOutOfBounds_ThrowsGameRuleException()
    {
        // Arrange
        var start = Coordinates.Create(0, 0);

        // Act
        void Act() => _ = start.GetCellsTowards(ShipDirection.Up, 2);

        // Assert
        Assert.Throws<GameRuleException>(Act);
    }

    [Fact]
    public void DistanceTo_ReturnsManhattanDistance()
    {
        // Arrange
        var first = Coordinates.Create(1, 2);
        var second = Coordinates.Create(4, 6);

        // Act
        var distance = first.DistanceTo(second);

        // Assert
        Assert.Equal(7, distance);
    }
}
