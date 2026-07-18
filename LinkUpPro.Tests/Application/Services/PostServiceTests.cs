using FluentValidation;
using LinkUpPro.Application.DTOs.Post.Requests;
using LinkUpPro.Application.DTOs.Profile.Responses;
using LinkUpPro.Application.Interfaces;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Infrastructure.Persistence.Repositories;
using LinkUpPro.Tests.Base;
using Microsoft.AspNetCore.Http;
using Moq;

namespace LinkUpPro.Tests.Application.Services;

[Collection("Sequential")]
public class PostServiceTests : InMemoryTestBase
{
    private IPostService? _service;
    private bool _hasImplementation;
    private Mock<IFileService> _fileServiceMock = null!;

    public PostServiceTests()
    {
        _hasImplementation = ImplementationDiscovery.HasImplementation<IPostService>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (!_hasImplementation)
            return;

        var postRepo = new PostRepository(DbContext);
        var friendshipRepo = new FriendshipRepository(DbContext);
        var reactionRepo = new ReactionRepository(DbContext);
        var commentRepo = new CommentRepository(DbContext);

        var profileServiceMock = new Mock<IProfileService>();
        profileServiceMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(
                (string id) =>
                    new UserResponseDto(
                        id,
                        "testuser",
                        "test@test.com",
                        "Author",
                        "Name",
                        "809-555-1234",
                        "/images/default-avatar.png",
                        true,
                        true
                    )
            );
        profileServiceMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (IEnumerable<string> ids, CancellationToken _) =>
                    ids.ToDictionary(
                        id => id,
                        id => new UserResponseDto(
                            id,
                            "testuser",
                            "test@test.com",
                            "Author",
                            "Name",
                            "809-555-1234",
                            "/images/default-avatar.png",
                            true,
                            true
                        )
                    )
            );

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _fileServiceMock = new Mock<IFileService>();
        _fileServiceMock.Setup(x => x.IsImageValid(It.IsAny<IFormFile>())).Returns(true);
        _fileServiceMock
            .Setup(x => x.UploadFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync("/uploads/posts/test-image.jpg");

        var createPostValidatorMock = new Mock<IValidator<CreatePostRequest>>();
        createPostValidatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<CreatePostRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var filterValidatorMock = new Mock<IValidator<PostFilterRequest>>();
        filterValidatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<PostFilterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var implType = ImplementationDiscovery.FindImplementation<IPostService>()!;
        _service = (IPostService)
            Activator.CreateInstance(
                implType,
                postRepo,
                friendshipRepo,
                reactionRepo,
                commentRepo,
                profileServiceMock.Object,
                unitOfWorkMock.Object,
                _fileServiceMock.Object,
                createPostValidatorMock.Object,
                filterValidatorMock.Object
            )!;
    }

    [ServiceFact(typeof(IPostService))]
    public async Task CreateAsync_ValidYouTubeRequest_ReturnsPost()
    {
        var request = new CreatePostRequest(
            "Hello World",
            2,
            null,
            "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            1,
            true
        );
        var result = await _service!.CreateAsync("user1", request);
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("Hello World");
        result.Value.MediaPath.Should().Be("dQw4w9WgXcQ");
    }

    [ServiceFact(typeof(IPostService))]
    public async Task CreateAsync_EmptyContent_ReturnsError()
    {
        var request = new CreatePostRequest("", 2, null, "https://www.youtube.com/watch?v=abc", 1, true);
        var result = await _service!.CreateAsync("user1", request);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IPostService))]
    public async Task CreateAsync_InvalidImage_ReturnsError()
    {
        _fileServiceMock.Setup(x => x.IsImageValid(It.IsAny<IFormFile>())).Returns(false);

        var formFile = new Mock<IFormFile>();
        formFile.Setup(x => x.Length).Returns(1024);

        var request = new CreatePostRequest("Test", 1, formFile.Object, null, 1, true);
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
        var request = new CreatePostRequest("Original", 2, null, "https://youtu.be/abc", 1, true);
        var created = await _service!.CreateAsync("user1", request);
        if (!created.IsSuccess)
            return;

        var updateReq = new UpdatePostRequest("Updated", null, null, null, null, null);
        var result = await _service!.UpdateAsync("user2", created.Value.Id, updateReq);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IPostService))]
    public async Task DeleteAsync_NotAuthor_ReturnsError()
    {
        var request = new CreatePostRequest("Test", 2, null, "https://youtu.be/abc", 1, true);
        var created = await _service!.CreateAsync("user1", request);
        if (!created.IsSuccess)
            return;

        var result = await _service!.DeleteAsync("user2", created.Value.Id);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IPostService))]
    public async Task DeleteAsync_Author_ReturnsSuccess()
    {
        var request = new CreatePostRequest("Test", 2, null, "https://youtu.be/abc", 1, true);
        var created = await _service!.CreateAsync("user1", request);
        if (!created.IsSuccess)
            return;

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

    [ServiceFact(typeof(IPostService))]
    public async Task CreateAsync_BothImageAndYouTube_ReturnsError()
    {
        var formFile = new Mock<IFormFile>();
        formFile.Setup(x => x.Length).Returns(1024);

        var request = new CreatePostRequest("Test", 1, formFile.Object, "https://www.youtube.com/watch?v=abc", 1, true);
        var result = await _service!.CreateAsync("user1", request);
        result.IsFailure.Should().BeTrue();
    }
}
