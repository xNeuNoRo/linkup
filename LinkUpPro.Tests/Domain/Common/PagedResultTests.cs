using LinkUpPro.Domain.Common;

namespace LinkUpPro.Tests.Domain.Common;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_WithRemainder_RoundsUp()
    {
        // Arrange
        var result = new PagedResult<int>([1, 2, 3], 11, 1, 5);

        // Act & Assert
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void HasNextPage_WhenPageIsBeforeLast_ReturnsTrue()
    {
        // Arrange
        var result = new PagedResult<int>([1, 2, 3], 11, 2, 5);

        // Act & Assert
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }
}
