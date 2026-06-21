using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Tests.Domain.Entities.Social;

public class PostTests
{
    [Fact]
    public void Create_ValidData_CreatesPostWithDefaults()
    {
        // Arrange & Act
        var result = CreatePost();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("author", result.Value.AuthorId);
        Assert.Equal(PrivacyLevel.FriendsOnly, result.Value.Privacy);
        Assert.True(result.Value.AllowComments);
        Assert.False(result.Value.IsEdited);
    }

    [Fact]
    public void Create_InvalidContent_ReturnsValidationErrors()
    {
        // Arrange & Act
        var result = Post.Create("author", " ", PostContentType.Image, "/img.webp");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "Post.ContentRequired");
    }

    [Fact]
    public void Create_ContentTooLong_ReturnsValidationError()
    {
        // Arrange
        var content = new string('a', DomainConstants.MaxPostContentLength + 1);

        // Act
        var result = Post.Create("author", content, PostContentType.Image, "/img.webp");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "Post.ContentTooLong");
    }

    [Fact]
    public void Edit_ValidData_UpdatesEditableFieldsAndMarksEdited()
    {
        // Arrange
        var post = CreatePost().Value;
        var updatedAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        // Act
        var result = post.Edit("Updated", PostContentType.YouTubeVideo, "dQw4w9WgXcQ", PrivacyLevel.OnlyMe, false, updatedAt);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Updated", post.Content);
        Assert.Equal(PostContentType.YouTubeVideo, post.ContentType);
        Assert.Equal("dQw4w9WgXcQ", post.MediaPath);
        Assert.Equal(PrivacyLevel.OnlyMe, post.Privacy);
        Assert.False(post.AllowComments);
        Assert.True(post.IsEdited);
        Assert.Equal(updatedAt, post.UpdatedAt);
    }

    [Fact]
    public void CanBeViewedBy_Author_ReturnsTrueEvenWhenOnlyMe()
    {
        // Arrange
        var post = Post.Create("author", "Content", PostContentType.Image, "/img.webp", PrivacyLevel.OnlyMe).Value;

        // Act & Assert
        Assert.True(post.CanBeViewedBy("author", isFriend: false));
    }

    [Fact]
    public void CanBeViewedBy_FriendAndFriendsOnly_ReturnsTrue()
    {
        // Arrange
        var post = CreatePost().Value;

        // Act & Assert
        Assert.True(post.CanBeViewedBy("friend", isFriend: true));
    }

    [Fact]
    public void CanBeViewedBy_NotFriendAndFriendsOnly_ReturnsFalse()
    {
        // Arrange
        var post = CreatePost().Value;

        // Act & Assert
        Assert.False(post.CanBeViewedBy("stranger", isFriend: false));
    }

    [Fact]
    public void MarkAsDeleted_DeletedPost_CannotBeViewedOrEdited()
    {
        // Arrange
        var post = CreatePost().Value;

        // Act
        post.MarkAsDeleted();

        // Assert
        Assert.False(post.CanBeViewedBy("author", isFriend: false));
        Assert.False(post.CanBeEditedBy("author"));
    }

    private static Result<Post> CreatePost() => Post.Create("author", "Content", PostContentType.Image, "/img.webp");
}
