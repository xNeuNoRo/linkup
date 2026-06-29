using LinkUpPro.Domain.Common;

namespace LinkUpPro.Tests.Domain.Common;

public class QueryOptionsTests
{
    [Fact]
    public void QueryOptions_DefaultValues_AreReadOptimized()
    {
        // Arrange & Act
        var options = new QueryOptions<TestEntity>();

        // Assert
        Assert.Null(options.Filter);
        Assert.Empty(options.Includes);
        Assert.Null(options.OrderBy);
        Assert.Null(options.Skip);
        Assert.Null(options.Take);
        Assert.False(options.IsTracking);
    }

    private sealed class TestEntity : BaseEntity<long>;
}
