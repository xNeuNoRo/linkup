using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;

namespace LinkUpPro.Tests.Domain.Entities.Social;

public class CommentTests
{
    [Fact]
    public void Create_WithoutParent_CreatesRootComment()
    {
        // Arrange & Act
        var result = Comment.Create(1, "user", "Hello");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsRootComment());
    }

    [Fact]
    public void Create_WithParent_CreatesReply()
    {
        // Arrange & Act
        var result = Comment.Create(1, "user", "Reply", parentCommentId: 10);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsRootComment());
        Assert.Equal(10, result.Value.ParentCommentId);
    }

    [Fact]
    public void Create_ContentTooLong_ReturnsValidationError()
    {
        // Arrange
        var content = new string('a', DomainConstants.MaxCommentContentLength + 1);

        // Act
        var result = Comment.Create(1, "user", content);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "Comment.ContentTooLong");
    }

    [Fact]
    public void Edit_ValidContent_UpdatesContentAndMarksEdited()
    {
        // Arrange
        var comment = Comment.Create(1, "user", "Old").Value;

        // Act
        var result = comment.Edit("New");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New", comment.Content);
        Assert.True(comment.IsEdited);
    }

    [Fact]
    public void MarkAsDeleted_WithReplies_ReplacesContentAndSoftDeletes()
    {
        // Arrange
        var comment = Comment.Create(1, "user", "Old").Value;

        // Act
        comment.MarkAsDeleted(hasReplies: true);

        // Assert
        Assert.True(comment.IsDeleted);
        Assert.Equal(Comment.DeletedCommentText, comment.Content);
    }
}
