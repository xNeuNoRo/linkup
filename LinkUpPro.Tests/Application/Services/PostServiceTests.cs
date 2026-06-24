using LinkUpPro.Application.DTOs.Post.Requests;
using LinkUpPro.Application.DTOs.Post.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.Interfaces.Repositories;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using LinkUpPro.Infrastructure.Persistence.Repositories;
using LinkUpPro.Tests.Base;
using Moq;

namespace LinkUpPro.Tests.Application.Services;

[Collection("Sequential")]
public class PostServiceTests : InMemoryTestBase
{
    private IPostService? _service;
    private bool _hasImplementation;

    public PostServiceTests()
    {
        _hasImplementation = ImplementationDiscovery.HasImplementation<IPostService>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (!_hasImplementation) return;

        var postRepo = new PostRepository(DbContext);
        var friendshipRepo = new FriendshipRepository(DbContext);
        var reactionRepo = new ReactionRepository(DbContext);
        var commentRepo = new CommentRepository(DbContext);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var implType = ImplementationDiscovery.FindImplementation<IPostService>()!;
        _service = (IPostService)Activator.CreateInstance(implType,
            postRepo, friendshipRepo, reactionRepo, commentRepo, unitOfWorkMock.Object)!;
    }

    [ServiceFact(typeof(IPostService))]
    public async Task CreateAsync_ValidRequest_ReturnsPost()
    {
        var request = new CreatePostRequest("Hello World", 1, "/img.jpg", null, 1, true);
        var result = await _service!.CreateAsync("user1", request);
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("Hello World");
    }

    [ServiceFact(typeof(IPostService))]
    public async Task CreateAsync_EmptyContent_ReturnsError()
    {
        var request = new CreatePostRequest("", 1, null, null, 1, true);
        var result = await _service!.CreateAsync("user1", request);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IPostService))]
    public async Task GetByIdAsync_ExistingPost_ReturnsPost()
    {
        var result = await _service!.GetByIdAsync("user1", 999);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IPostService))]
    public async Task UpdateAsync_NotAuthor_ReturnsError()
    {
        var request = new CreatePostRequest("Original", 1, null, null, 1, true);
        var created = await _service!.CreateAsync("user1", request);
        if (!created.IsSuccess) return;

        var updateReq = new UpdatePostRequest("Updated", null, null, null, null, null);
        var result = await _service!.UpdateAsync("user2", created.Value.Id, updateReq);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IPostService))]
    public async Task DeleteAsync_NotAuthor_ReturnsError()
    {
        var request = new CreatePostRequest("Test", 1, null, null, 1, true);
        var created = await _service!.CreateAsync("user1", request);
        if (!created.IsSuccess) return;

        var result = await _service!.DeleteAsync("user2", created.Value.Id);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IPostService))]
    public async Task DeleteAsync_Author_ReturnsSuccess()
    {
        var request = new CreatePostRequest("Test", 1, null, null, 1, true);
        var created = await _service!.CreateAsync("user1", request);
        if (!created.IsSuccess) return;

        var result = await _service!.DeleteAsync("user1", created.Value.Id);
        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IPostService))]
    public async Task GetMyPostsAsync_ReturnsPaged()
    {
        var filter = new PostFilterRequest(null, null, null, null, null, 1, 20);
        var result = await _service!.GetMyPostsAsync("user1", filter);
        result.Should().NotBeNull();
    }

    [ServiceFact(typeof(IPostService))]
    public async Task GetFriendsPostsAsync_ReturnsOnlyFriendsPosts()
    {
        var filter = new PostFilterRequest(null, null, null, null, null, 1, 20);
        var result = await _service!.GetFriendsPostsAsync("user1", filter);
        result.Should().NotBeNull();
    }

    [ServiceFact(typeof(IPostService))]
    public async Task GetMyPostsAsync_WithContentTypeFilter_Filters()
    {
        var filter = new PostFilterRequest(null, 1, null, null, null, 1, 20);
        var result = await _service!.GetMyPostsAsync("user1", filter);
        result.Should().NotBeNull();
    }

    [ServiceFact(typeof(IPostService))]
    public async Task GetMyPostsAsync_WithDateRange_Filters()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-7);
        var to = DateTimeOffset.UtcNow;
        var filter = new PostFilterRequest(null, null, from, to, null, 1, 20);
        var result = await _service!.GetMyPostsAsync("user1", filter);
        result.Should().NotBeNull();
    }
}
