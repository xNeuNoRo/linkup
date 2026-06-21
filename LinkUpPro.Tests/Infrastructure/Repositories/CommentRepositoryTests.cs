using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Infrastructure.Persistence.Repositories;

namespace LinkUpPro.Tests.Infrastructure.Repositories;

public sealed class CommentRepositoryTests : PersistenceTestBase
{
    [Fact]
    public async Task GetByPostAsync_ReturnsPostComments()
    {
        var context = CreateContext();
        var repo = new CommentRepository(context);
        var author = await SeedUserAsync(context);

        var post = Post.Create(author.Id, "Post", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var comment1 = Comment.Create(post.Id, author.Id, "Comment 1").Value;
        var comment2 = Comment.Create(post.Id, author.Id, "Comment 2").Value;
        context.Comments.AddRange(comment1, comment2);
        await context.SaveChangesAsync();

        var result = await repo.GetByPostAsync(post.Id);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRepliesAsync_ExistingParent_ReturnsReplies()
    {
        var context = CreateContext();
        var repo = new CommentRepository(context);
        var author = await SeedUserAsync(context);

        var post = Post.Create(author.Id, "Post", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var root = Comment.Create(post.Id, author.Id, "Root").Value;
        context.Comments.Add(root);
        await context.SaveChangesAsync();

        var reply = Comment.Create(post.Id, author.Id, "Reply", parentCommentId: root.Id).Value;
        context.Comments.Add(reply);
        await context.SaveChangesAsync();

        var result = await repo.GetRepliesAsync(root.Id);

        result.Should().HaveCount(1);
        result.First().Content.Should().Be("Reply");
    }

    [Fact]
    public async Task GetThreadAsync_RootComment_ReturnsThreadHierarchy()
    {
        var context = CreateContext();
        var repo = new CommentRepository(context);
        var author = await SeedUserAsync(context);

        var post = Post.Create(author.Id, "Post", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var root = Comment.Create(post.Id, author.Id, "Root").Value;
        context.Comments.Add(root);
        await context.SaveChangesAsync();

        var reply1 = Comment.Create(post.Id, author.Id, "Reply 1", parentCommentId: root.Id).Value;
        context.Comments.Add(reply1);
        await context.SaveChangesAsync();

        var reply2 = Comment
            .Create(post.Id, author.Id, "Reply 2", parentCommentId: reply1.Id)
            .Value;
        context.Comments.Add(reply2);
        await context.SaveChangesAsync();

        var result = await repo.GetThreadAsync(root.Id);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task CountByPostAsync_ReturnsCorrectCount()
    {
        var context = CreateContext();
        var repo = new CommentRepository(context);
        var author = await SeedUserAsync(context);

        var post = Post.Create(author.Id, "Post", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        context.Comments.AddRange(
            Comment.Create(post.Id, author.Id, "C1").Value,
            Comment.Create(post.Id, author.Id, "C2").Value,
            Comment.Create(post.Id, author.Id, "C3").Value
        );
        await context.SaveChangesAsync();

        var count = await repo.CountByPostAsync(post.Id);

        count.Should().Be(3);
    }

    [Fact]
    public async Task HasRepliesAsync_WithReplies_ReturnsTrue()
    {
        var context = CreateContext();
        var repo = new CommentRepository(context);
        var author = await SeedUserAsync(context);

        var post = Post.Create(author.Id, "Post", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var root = Comment.Create(post.Id, author.Id, "Root").Value;
        context.Comments.Add(root);
        await context.SaveChangesAsync();

        context.Comments.Add(
            Comment.Create(post.Id, author.Id, "Reply", parentCommentId: root.Id).Value
        );
        await context.SaveChangesAsync();

        var hasReplies = await repo.HasRepliesAsync(root.Id);

        hasReplies.Should().BeTrue();
    }

    [Fact]
    public async Task HasRepliesAsync_NoReplies_ReturnsFalse()
    {
        var context = CreateContext();
        var repo = new CommentRepository(context);
        var author = await SeedUserAsync(context);

        var post = Post.Create(author.Id, "Post", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var root = Comment.Create(post.Id, author.Id, "Root").Value;
        context.Comments.Add(root);
        await context.SaveChangesAsync();

        var hasReplies = await repo.HasRepliesAsync(root.Id);

        hasReplies.Should().BeFalse();
    }

    [Fact]
    public async Task SoftDelete_ExcludesDeletedComments()
    {
        var context = CreateContext();
        var repo = new CommentRepository(context);
        var author = await SeedUserAsync(context);

        var post = Post.Create(author.Id, "Post", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var comment = Comment.Create(post.Id, author.Id, "To delete").Value;
        context.Comments.Add(comment);
        await context.SaveChangesAsync();

        comment.MarkAsDeleted(hasReplies: false);
        await context.SaveChangesAsync();

        var result = await repo.GetByPostAsync(post.Id);

        result.Should().BeEmpty();
    }
}
