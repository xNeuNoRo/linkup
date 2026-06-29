using DomainFriendship = LinkUpPro.Domain.Entities.Friendship.Friendship;

namespace LinkUpPro.Tests.Domain.Entities.Friendship;

public class FriendshipTests
{
    [Fact]
    public void Create_ValidUsers_OrdersUserIds()
    {
        // Arrange & Act
        var result = DomainFriendship.Create("user-b", "user-a");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("user-a", result.Value.User1Id);
        Assert.Equal("user-b", result.Value.User2Id);
    }

    [Fact]
    public void Create_SameUser_ReturnsFailure()
    {
        // Arrange & Act
        var result = DomainFriendship.Create("user-a", "user-a");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "Friendship.SelfFriendshipNotAllowed");
    }

    [Fact]
    public void MarkAsDeleted_ThenReactivate_RestoresFriendship()
    {
        // Arrange
        var friendship = DomainFriendship.Create("user-a", "user-b").Value;
        friendship.MarkAsDeleted();

        // Act
        friendship.Reactivate();

        // Assert
        Assert.False(friendship.IsDeleted);
        Assert.Null(friendship.DeletedAt);
        Assert.NotNull(friendship.UpdatedAt);
    }

    [Fact]
    public void GetFriendId_UserInFriendship_ReturnsOtherUser()
    {
        // Arrange
        var friendship = DomainFriendship.Create("user-a", "user-b").Value;

        // Act
        var result = friendship.GetFriendId("user-a");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("user-b", result.Value);
    }

    [Fact]
    public void GetFriendId_UserOutsideFriendship_ReturnsFailure()
    {
        // Arrange
        var friendship = DomainFriendship.Create("user-a", "user-b").Value;

        // Act
        var result = friendship.GetFriendId("user-c");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Friendship.UserNotInFriendship", result.Error.Code);
    }
}
