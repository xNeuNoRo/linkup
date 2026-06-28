using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Tests.Domain.Entities.Social;

public class NotificationTests
{
    [Fact]
    public void CreateComment_ValidData_CreatesUnreadNotification()
    {
        // Arrange & Act
        var result = Notification.CreateComment("recipient", "actor", 1, "Juan123");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsRead);
        Assert.Equal(NotificationType.Comment, result.Value.Type);
        Assert.Equal("Juan123 comento tu publicacion.", result.Value.Message);
    }

    [Fact]
    public void CreateReaction_ValidData_UsesReactionDisplayName()
    {
        // Arrange & Act
        var result = Notification.CreateReaction("recipient", "actor", 1, "Pedro89", ReactionType.Dislike);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Pedro89 reacciono con No me gusta a tu publicacion.", result.Value.Message);
    }

    [Fact]
    public void CreateComment_SameRecipientAndActor_ReturnsFailure()
    {
        // Arrange & Act
        var result = Notification.CreateComment("user", "user", 1, "Juan123");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "Notification.SelfNotificationNotAllowed");
    }

    [Fact]
    public void MarkAsRead_UnreadNotification_MarksAsRead()
    {
        // Arrange
        var notification = Notification.CreateComment("recipient", "actor", 1, "Juan123").Value;

        // Act
        notification.MarkAsRead();

        // Assert
        Assert.True(notification.IsRead);
        Assert.NotNull(notification.UpdatedAt);
    }
}
