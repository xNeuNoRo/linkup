using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Tests.Domain.Enums;

public class ShipDirectionTests
{
    [Fact]
    public void ShipDirection_Values_MatchDatabaseContract()
    {
        // Arrange & Act & Assert
        Assert.Equal(1, (int)ShipDirection.Up);
        Assert.Equal(2, (int)ShipDirection.Down);
        Assert.Equal(3, (int)ShipDirection.Left);
        Assert.Equal(4, (int)ShipDirection.Right);
    }
}
