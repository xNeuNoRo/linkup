using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Tests.Domain.Enums;

public class GameStatusTests
{
    [Fact]
    public void GameStatus_Values_MatchDatabaseContract()
    {
        // Arrange & Act & Assert
        Assert.Equal(1, (int)GameStatus.Configuring_P1);
        Assert.Equal(2, (int)GameStatus.Configuring_P2);
        Assert.Equal(3, (int)GameStatus.InProgress);
        Assert.Equal(4, (int)GameStatus.Finished_Winner);
        Assert.Equal(5, (int)GameStatus.Finished_Abandoned);
    }
}
