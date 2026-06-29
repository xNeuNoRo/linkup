using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Tests.Domain.Enums;

public class ShipSizeTests
{
    [Fact]
    public void ShipSize_Values_MatchFunctionalFleetSizes()
    {
        // Arrange & Act & Assert
        Assert.Equal(2, (int)ShipSize.Size2);
        Assert.Equal(3, (int)ShipSize.Size3);
        Assert.Equal(4, (int)ShipSize.Size4);
        Assert.Equal(5, (int)ShipSize.Size5);
    }
}
