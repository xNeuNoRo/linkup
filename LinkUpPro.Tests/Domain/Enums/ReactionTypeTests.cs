using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Tests.Domain.Enums;

public class ReactionTypeTests
{
    [Fact]
    public void ReactionType_Values_MatchDatabaseContract()
    {
        // Arrange & Act & Assert
        Assert.Equal(1, (int)ReactionType.Like);
        Assert.Equal(2, (int)ReactionType.Dislike);
    }
}
