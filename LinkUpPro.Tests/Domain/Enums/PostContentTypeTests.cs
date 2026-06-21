using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Tests.Domain.Enums;

public class PostContentTypeTests
{
    [Fact]
    public void PostContentType_Values_MatchDatabaseContract()
    {
        // Arrange & Act & Assert
        Assert.Equal(1, (int)PostContentType.Image);
        Assert.Equal(2, (int)PostContentType.YouTubeVideo);
    }
}
