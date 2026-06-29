using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Infrastructure.Persistence.Repositories;

namespace LinkUpPro.Tests.Infrastructure.Repositories;

public sealed class GenericRepositoryTests : PersistenceTestBase
{
    [Fact]
    public async Task GetAllAsync_NoOptions_ReturnsAllEntities()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);

        var post1 = Post.Create("author1", "Content 1", PostContentType.Image, "/img1.jpg");
        var post2 = Post.Create("author2", "Content 2", PostContentType.Image, "/img2.jpg");
        context.Posts.AddRange(post1.Value, post2.Value);
        await context.SaveChangesAsync();

        var result = await repo.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsEntity()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);

        var post = Post.Create("author1", "Content", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var result = await repo.GetByIdAsync(post.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(post.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);

        var result = await repo.GetByIdAsync(999L);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ValidEntity_PersistsAndCanBeRetrieved()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);

        var post = Post.Create("author1", "Content", PostContentType.Image, "/img.jpg").Value;
        await repo.AddAsync(post);
        await context.SaveChangesAsync();

        var retrieved = await repo.GetByIdAsync(post.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Content.Should().Be("Content");
    }

    [Fact]
    public async Task AddRangeAsync_MultipleEntities_AllPersisted()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);

        var posts = new[]
        {
            Post.Create("a1", "C1", PostContentType.Image, "/img1.jpg").Value,
            Post.Create("a2", "C2", PostContentType.Image, "/img2.jpg").Value,
            Post.Create("a3", "C3", PostContentType.Image, "/img3.jpg").Value,
        };

        await repo.AddRangeAsync(posts);
        await context.SaveChangesAsync();

        var all = await repo.GetAllAsync();
        all.Should().HaveCount(3);
    }

    [Fact]
    public async Task Update_ExistingEntity_UpdatesProperties()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);

        var post = Post.Create("author1", "Original", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var editResult = post.Edit("Updated", PostContentType.Image, "/img.jpg", PrivacyLevel.FriendsOnly, true);
        repo.Update(post);
        await context.SaveChangesAsync();

        var retrieved = await repo.GetByIdAsync(post.Id);
        retrieved!.Content.Should().Be("Updated");
        retrieved.IsEdited.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ExistingEntity_ReturnsTrue()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);

        var post = Post.Create("author1", "Content", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var exists = await repo.ExistsAsync(p => p.Id == post.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExisting_ReturnsFalse()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);

        var exists = await repo.ExistsAsync(p => p.Id == 999L);

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_NoPredicate_ReturnsTotalCount()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);

        context.Posts.AddRange(
            Post.Create("a1", "C1", PostContentType.Image, "/img1.jpg").Value,
            Post.Create("a2", "C2", PostContentType.Image, "/img2.jpg").Value);
        await context.SaveChangesAsync();

        var count = await repo.CountAsync();

        count.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_WithQueryOptions_FilterWorks()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);

        context.Posts.AddRange(
            Post.Create("auth1", "Alpha", PostContentType.Image, "/img1.jpg").Value,
            Post.Create("auth2", "Beta", PostContentType.Image, "/img2.jpg").Value);
        await context.SaveChangesAsync();

        var options = new QueryOptions<Post> { Filter = p => p.Content == "Alpha" };
        var result = await repo.GetAllAsync(options);

        result.Should().HaveCount(1);
        result.First().Content.Should().Be("Alpha");
    }

    [Fact]
    public async Task GetAllAsync_WithQueryOptions_SkipTakeWorks()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);

        context.Posts.AddRange(
            Post.Create("a1", "C1", PostContentType.Image, "/img1.jpg").Value,
            Post.Create("a2", "C2", PostContentType.Image, "/img2.jpg").Value,
            Post.Create("a3", "C3", PostContentType.Image, "/img3.jpg").Value);
        await context.SaveChangesAsync();

        var options = new QueryOptions<Post>
        {
            OrderBy = q => q.OrderBy(p => p.Content),
            Skip = 1,
            Take = 1,
        };

        var result = await repo.GetAllAsync(options);

        result.Should().HaveCount(1);
        result.First().Content.Should().Be("C2");
    }
}
