using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Tests.Domain.ValueObjects;

public class BoardCellTests
{
    [Theory]
    [InlineData(BoardCellState.Hit, true)]
    [InlineData(BoardCellState.Miss, true)]
    [InlineData(BoardCellState.Empty, false)]
    [InlineData(BoardCellState.Ship, false)]
    public void IsAttackResult_ReturnsExpectedValue(BoardCellState state, bool expected)
    {
        // Arrange
        var cell = new BoardCell(Coordinates.Create(1, 1), state);

        // Act & Assert
        Assert.Equal(expected, cell.IsAttackResult);
    }
}
