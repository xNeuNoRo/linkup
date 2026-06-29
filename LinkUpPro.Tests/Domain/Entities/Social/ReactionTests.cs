using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Tests.Domain.Entities.Social;

public class ReactionTests
{
    [Fact]
    public void Create_ValidData_CreatesReaction()
    {
        // Arrange & Act
        var result = Reaction.Create(1, "user", ReactionType.Like);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ReactionType.Like, result.Value.Type);
    }

    [Fact]
    public void ChangeTo_DifferentType_UpdatesReaction()
    {
        // Arrange
        var reaction = Reaction.Create(1, "user", ReactionType.Like).Value;

        // Act
        var result = reaction.ChangeTo(ReactionType.Dislike);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ReactionType.Dislike, reaction.Type);
        Assert.NotNull(reaction.UpdatedAt);
    }
}
