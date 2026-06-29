using LinkUpPro.Domain.Common;

namespace LinkUpPro.Tests.Domain.Common;

public class DomainConstantsTests
{
    [Fact]
    public void DomainConstants_PdfLimits_AreConfiguredCorrectly()
    {
        // Arrange & Act & Assert
        Assert.Equal(1_000, DomainConstants.MaxPostContentLength);
        Assert.Equal(500, DomainConstants.MaxCommentContentLength);
        Assert.Equal(12, DomainConstants.BoardSize);
        Assert.Equal(TimeSpan.FromHours(48), DomainConstants.BattleshipTurnTimeout);
        Assert.Equal(5 * 1024 * 1024, DomainConstants.MaxImageFileSizeBytes);
    }

    [Fact]
    public void RequiredBattleshipFleetSizes_MatchesRequirements()
    {
        // Arrange & Act & Assert
        Assert.Equal([5, 4, 3, 3, 2], DomainConstants.RequiredBattleshipFleetSizes);
    }

    [Theory]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    [InlineData(".png")]
    [InlineData(".webp")]
    [InlineData(".JPG")]
    public void AllowedImageExtensions_ContainsRequiredExtensions_IgnoringCase(string extension)
    {
        // Arrange & Act & Assert
        Assert.Contains(extension, DomainConstants.AllowedImageExtensions);
    }
}
