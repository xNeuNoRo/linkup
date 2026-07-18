using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Settings;

namespace LinkUpPro.Tests.Domain.Settings;

public class GameSettingsTests
{
    [Fact]
    public void Defaults_MatchBattleshipFunctionalDocument()
    {
        // Arrange & Act
        var settings = new GameSettings();

        // Assert
        Assert.Equal(DomainConstants.BoardSize, settings.BoardSize);
        Assert.Equal(48, settings.TurnTimeoutHours);
        Assert.Equal(DomainConstants.BattleshipTurnTimeout, settings.TurnTimeout);
        Assert.Equal([5, 4, 3, 3, 2], settings.RequiredFleetSizes);
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(11, 11, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(12, 0, false)]
    [InlineData(0, 12, false)]
    public void IsValidCoordinate_ReturnsExpectedValue(int x, int y, bool expected)
    {
        // Arrange
        var settings = new GameSettings();

        // Act
        var isValid = settings.IsValidCoordinate(x, y);

        // Assert
        Assert.Equal(expected, isValid);
    }

    [Theory]
    [InlineData(new[] { 5, 4, 3, 3, 2 }, true)]
    [InlineData(new[] { 2, 3, 3, 4, 5 }, true)]
    [InlineData(new[] { 5, 4, 3, 2 }, false)]
    [InlineData(new[] { 5, 4, 4, 3, 2 }, false)]
    public void HasValidFleetComposition_ReturnsExpectedValue(int[] fleetSizes, bool expected)
    {
        // Arrange
        var settings = new GameSettings();

        // Act
        var isValid = settings.HasValidFleetComposition(fleetSizes);

        // Assert
        Assert.Equal(expected, isValid);
    }
}
