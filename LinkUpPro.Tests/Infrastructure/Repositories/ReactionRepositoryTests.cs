using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Infrastructure.Persistence.Repositories;

namespace LinkUpPro.Tests.Infrastructure.Repositories;

public sealed class ReactionRepositoryTests : PersistenceTestBase
{
    [Fact]
    public async Task GetByPostAndUserAsync_Existing_ReturnsReaction()
    {
        var context = CreateContext();
        var repo = new ReactionRepository(context);
        var user = await SeedUserAsync(context);

        var post = Post.Create(user.Id, "Post", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var reaction = Reaction.Create(post.Id, user.Id, ReactionType.Like).Value;
        context.Reactions.Add(reaction);
        await context.SaveChangesAsync();

        var result = await repo.GetByPostAndUserAsync(post.Id, user.Id);

        result.Should().NotBeNull();
        result!.Type.Should().Be(ReactionType.Like);
    }

    [Fact]
    public async Task ExistsByPostAndUserAsync_Existing_ReturnsTrue()
    {
        var context = CreateContext();
        var repo = new ReactionRepository(context);
        var user = await SeedUserAsync(context);

        var post = Post.Create(user.Id, "Post", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        context.Reactions.Add(Reaction.Create(post.Id, user.Id, ReactionType.Like).Value);
        await context.SaveChangesAsync();

        var exists = await repo.ExistsByPostAndUserAsync(post.Id, user.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetCountsByPostAsync_ReturnsCorrectCounts()
    {
        var context = CreateContext();
        var repo = new ReactionRepository(context);
        var user1 = await SeedUserAsync(context, "user1", "u1@t.com");
        var user2 = await SeedUserAsync(context, "user2", "u2@t.com");

        var post = Post.Create(user1.Id, "Post", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        context.Reactions.AddRange(
            Reaction.Create(post.Id, user1.Id, ReactionType.Like).Value,
            Reaction.Create(post.Id, user2.Id, ReactionType.Dislike).Value
        );
        await context.SaveChangesAsync();

        var counts = await repo.GetCountsByPostAsync(post.Id);

        counts.Likes.Should().Be(1);
        counts.Dislikes.Should().Be(1);
        counts.Total.Should().Be(2);
    }

    [Fact]
    public async Task CountByPostAndTypeAsync_ReturnsCorrectCount()
    {
        var context = CreateContext();
        var repo = new ReactionRepository(context);
        var user = await SeedUserAsync(context, "user1", "u1@t.com");
        var user2 = await SeedUserAsync(context, "user2", "u2@t.com");

        var post = Post.Create(user.Id, "Post", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        context.Reactions.AddRange(
            Reaction.Create(post.Id, user.Id, ReactionType.Like).Value,
            Reaction.Create(post.Id, user2.Id, ReactionType.Like).Value
        );
        await context.SaveChangesAsync();

        var count = await repo.CountByPostAndTypeAsync(post.Id, ReactionType.Like);

        count.Should().Be(2);
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsUsersReactions()
    {
        var context = CreateContext();
        var repo = new ReactionRepository(context);
        var user = await SeedUserAsync(context);

        var post1 = Post.Create(user.Id, "P1", PostContentType.Image, "/1.jpg").Value;
        var post2 = Post.Create(user.Id, "P2", PostContentType.Image, "/2.jpg").Value;
        context.Posts.AddRange(post1, post2);
        await context.SaveChangesAsync();

        context.Reactions.AddRange(
            Reaction.Create(post1.Id, user.Id, ReactionType.Like).Value,
            Reaction.Create(post2.Id, user.Id, ReactionType.Dislike).Value
        );
        await context.SaveChangesAsync();

        var result = await repo.GetByUserAsync(user.Id);

        result.Should().HaveCount(2);
    }
}
