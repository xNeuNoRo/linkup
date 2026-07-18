using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Tests.Domain.Enums;

public class PrivacyLevelTests
{
    [Fact]
    public void PrivacyLevel_Values_MatchDatabaseContract()
    {
        // Arrange & Act & Assert
        Assert.Equal(1, (int)PrivacyLevel.FriendsOnly);
        Assert.Equal(2, (int)PrivacyLevel.OnlyMe);
    }
}
