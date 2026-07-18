using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Tests.Domain.Entities.Social;

public class NotificationTests
{
    [Fact]
    public void CreateComment_ValidData_CreatesUnreadNotificationWithPostType()
    {
        // Arrange & Act
        var result = Notification.CreateComment("recipient", "actor", 1, "Juan123");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsRead);
        Assert.Equal(NotificationType.Comment, result.Value.Type);
        Assert.Equal(RelatedEntityType.Post, result.Value.RelatedEntityType);
        Assert.Equal(1, result.Value.RelatedEntityId);
        Assert.Equal("Juan123 comento tu publicacion.", result.Value.Message);
    }

    [Fact]
    public void CreateReply_ValidData_SetsPostType()
    {
        // Arrange & Act
        var result = Notification.CreateReply("recipient", "actor", 42, "Maria45");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(NotificationType.Reply, result.Value.Type);
        Assert.Equal(RelatedEntityType.Post, result.Value.RelatedEntityType);
        Assert.Equal(42, result.Value.RelatedEntityId);
    }

    [Fact]
    public void CreateReaction_ValidData_UsesReactionDisplayName()
    {
        // Arrange & Act
        var result = Notification.CreateReaction("recipient", "actor", 1, "Pedro89", ReactionType.Dislike);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(RelatedEntityType.Post, result.Value.RelatedEntityType);
        Assert.Equal("Pedro89 reacciono con No me gusta a tu publicacion.", result.Value.Message);
    }

    [Fact]
    public void CreateFriendRequestSent_ValidData_SetsFriendRequestType()
    {
        // Arrange & Act
        var result = Notification.CreateFriendRequestSent("recipient", "actor", 99, "UserX");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(NotificationType.FriendRequestSent, result.Value.Type);
        Assert.Equal(RelatedEntityType.FriendRequest, result.Value.RelatedEntityType);
        Assert.Equal(99, result.Value.RelatedEntityId);
    }

    [Fact]
    public void CreateFriendRequestAccepted_ValidData_SetsFriendRequestType()
    {
        // Arrange & Act
        var result = Notification.CreateFriendRequestAccepted("recipient", "actor", 99, "UserX");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(NotificationType.FriendRequestAccepted, result.Value.Type);
        Assert.Equal(RelatedEntityType.FriendRequest, result.Value.RelatedEntityType);
    }

    [Fact]
    public void CreateFriendRequestRejected_ValidData_SetsFriendRequestType()
    {
        // Arrange & Act
        var result = Notification.CreateFriendRequestRejected("recipient", "actor", 99, "UserX");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(NotificationType.FriendRequestRejected, result.Value.Type);
        Assert.Equal(RelatedEntityType.FriendRequest, result.Value.RelatedEntityType);
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
    public void Create_WithRelatedEntityIdButNoneType_ReturnsFailure()
    {
        // Arrange & Act
        var result = Notification.Create(
            "recipient",
            "actor",
            NotificationType.Comment,
            "Test message",
            RelatedEntityType.None,
            relatedEntityId: 1
        );

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "Notification.EntityTypeRequired");
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
