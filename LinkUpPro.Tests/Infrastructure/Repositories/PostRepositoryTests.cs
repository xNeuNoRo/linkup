using LinkUpPro.Domain.Entities.Friendship;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Infrastructure.Persistence.Repositories;

namespace LinkUpPro.Tests.Infrastructure.Repositories;

public sealed class PostRepositoryTests : PersistenceTestBase
{
    [Fact]
    public async Task GetByAuthorAsync_ReturnsAuthorsPosts()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);
        var author = await SeedUserAsync(context, "author");

        var post1 = Post.Create(author.Id, "Post 1", PostContentType.Image, "/img1.jpg").Value;
        var post2 = Post.Create(
            author.Id,
            "Post 2",
            PostContentType.YouTubeVideo,
            "https://youtu.be/abc"
        ).Value;
        context.Posts.AddRange(post1, post2);
        await context.SaveChangesAsync();

        var result = await repo.GetByAuthorAsync(author.Id);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdWithDetailsAsync_ExistingPost_ReturnsPost()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);
        var author = await SeedUserAsync(context);

        var post = Post.Create(author.Id, "Test content", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var result = await repo.GetByIdWithDetailsAsync(post.Id);

        result.Should().NotBeNull();
        result!.Content.Should().Be("Test content");
    }

    [Fact]
    public async Task GetVisibleFriendsPostsAsync_ReturnsFriendPosts()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);

        var userA = await SeedUserAsync(context, "userA", "a@t.com");
        var userB = await SeedUserAsync(context, "userB", "b@t.com");

        var friendship = Friendship.Create(userA.Id, userB.Id).Value;
        context.Friendships.Add(friendship);

        var post = Post.Create(
            userB.Id,
            "Friend post",
            PostContentType.Image,
            "/img.jpg",
            privacy: PrivacyLevel.FriendsOnly
        ).Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var result = await repo.GetVisibleFriendsPostsAsync(userA.Id);

        result.Should().HaveCount(1);
        result.First().Content.Should().Be("Friend post");
    }

    [Fact]
    public async Task GetVisibleFriendsPostsAsync_OnlyMePost_NotReturned()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);

        var userA = await SeedUserAsync(context, "userA", "a@t.com");
        var userB = await SeedUserAsync(context, "userB", "b@t.com");

        var friendship = Friendship.Create(userA.Id, userB.Id).Value;
        context.Friendships.Add(friendship);

        var post = Post.Create(
            userB.Id,
            "Private post",
            PostContentType.Image,
            "/img.jpg",
            privacy: PrivacyLevel.OnlyMe
        ).Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var result = await repo.GetVisibleFriendsPostsAsync(userA.Id);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAuthorPostsAsync_ByText_ReturnsMatches()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);
        var author = await SeedUserAsync(context, "author");

        var post1 = Post.Create(author.Id, "Hello World", PostContentType.Image, "/img1.jpg").Value;
        var post2 = Post.Create(
            author.Id,
            "Goodbye World",
            PostContentType.Image,
            "/img2.jpg"
        ).Value;
        context.Posts.AddRange(post1, post2);
        await context.SaveChangesAsync();

        var result = await repo.SearchAuthorPostsAsync(
            authorId: author.Id,
            searchText: "Hello",
            contentType: null,
            fromDate: null,
            toDate: null,
            editedOnly: null
        );

        result.Should().HaveCount(1);
        result.First().Content.Should().Be("Hello World");
    }

    [Fact]
    public async Task SearchAuthorPostsAsync_ByContentType_ReturnsFiltered()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);
        var author = await SeedUserAsync(context);

        context.Posts.AddRange(
            Post.Create(author.Id, "Image post", PostContentType.Image, "/img.jpg").Value,
            Post.Create(
                author.Id,
                "Video post",
                PostContentType.YouTubeVideo,
                "https://youtu.be/abc"
            ).Value
        );
        await context.SaveChangesAsync();

        var result = await repo.SearchAuthorPostsAsync(
            authorId: author.Id,
            searchText: null,
            contentType: PostContentType.YouTubeVideo,
            fromDate: null,
            toDate: null,
            editedOnly: null
        );

        result.Should().HaveCount(1);
        result.First().ContentType.Should().Be(PostContentType.YouTubeVideo);
    }

    [Fact]
    public async Task SearchAuthorPostsAsync_ByDateRange_ReturnsFiltered()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);
        var author = await SeedUserAsync(context);

        context.Posts.Add(
            Post.Create(author.Id, "Old post", PostContentType.Image, "/img.jpg").Value
        );
        await context.SaveChangesAsync();

        var fromDate = TestDateTimeProvider.FixedUtcNow.AddDays(-1);
        var toDate = TestDateTimeProvider.FixedUtcNow.AddDays(-1);

        var result = await repo.SearchAuthorPostsAsync(
            authorId: author.Id,
            searchText: null,
            contentType: null,
            fromDate: fromDate,
            toDate: toDate,
            editedOnly: null
        );

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SoftDelete_ExcludesFromGetByAuthor()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);
        var author = await SeedUserAsync(context);

        var post = Post.Create(author.Id, "To delete", PostContentType.Image, "/img.jpg").Value;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        post.MarkAsDeleted();
        await context.SaveChangesAsync();

        var result = await repo.GetByAuthorAsync(author.Id);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchFriendsPostsAsync_WithFriendFilter_ReturnsFiltered()
    {
        var context = CreateContext();
        var repo = new PostRepository(context);

        var userA = await SeedUserAsync(context, "userA", "a@t.com");
        var userB = await SeedUserAsync(context, "userB", "b@t.com");
        var userC = await SeedUserAsync(context, "userC", "c@t.com");

        context.Friendships.Add(Friendship.Create(userA.Id, userB.Id).Value);
        context.Friendships.Add(Friendship.Create(userA.Id, userC.Id).Value);

        context.Posts.AddRange(
            Post.Create(
                userB.Id,
                "B's post",
                PostContentType.Image,
                "/b.jpg",
                PrivacyLevel.FriendsOnly
            ).Value,
            Post.Create(
                userC.Id,
                "C's post",
                PostContentType.Image,
                "/c.jpg",
                PrivacyLevel.FriendsOnly
            ).Value
        );
        await context.SaveChangesAsync();

        var result = await repo.SearchFriendsPostsAsync(
            userId: userA.Id,
            searchText: null,
            friendId: userB.Id,
            contentType: null,
            fromDate: null,
            toDate: null,
            editedOnly: null
        );

        result.Should().HaveCount(1);
        result.First().Content.Should().Be("B's post");
    }
}
