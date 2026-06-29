using LinkUpPro.Application.DTOs.Comment.Requests;
using LinkUpPro.Application.DTOs.Profile.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.Interfaces.Repositories;
using LinkUpPro.Infrastructure.Persistence.Repositories;
using LinkUpPro.Tests.Base;
using Moq;
using DomainFriendship = LinkUpPro.Domain.Entities.Friendship.Friendship;

namespace LinkUpPro.Tests.Application.Services;

[Collection("Sequential")]
public class CommentServiceTests : InMemoryTestBase
{
    private ICommentService? _service;
    private Mock<IProfileService> _profileServiceMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private Mock<INotificationRepository> _notificationRepositoryMock = null!;
    private bool _hasImplementation;

    public CommentServiceTests()
    {
        _hasImplementation = ImplementationDiscovery.HasImplementation<ICommentService>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (!_hasImplementation)
            return;

        var post = Post.Create(
            "author1",
            "Test post",
            PostContentType.Image,
            "/img.jpg",
            PrivacyLevel.FriendsOnly,
            true
        ).Value;
        typeof(Post).GetProperty(nameof(Post.Id))!.SetValue(post, 1L);

        var comment = Comment.Create(1, "author1", "Test comment", null).Value;
        typeof(Comment).GetProperty(nameof(Comment.Id))!.SetValue(comment, 1L);

        var friendship = DomainFriendship.Create("user1", "author1").Value;

        DbContext.Add(post);
        DbContext.Add(comment);
        DbContext.Add(friendship);
        await DbContext.SaveChangesAsync();

        var commentRepo = new CommentRepository(DbContext);
        var postRepo = new PostRepository(DbContext);
        var friendshipRepo = new FriendshipRepository(DbContext);

        _profileServiceMock = new Mock<IProfileService>();
        _profileServiceMock
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
        _profileServiceMock
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

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _notificationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var implType = ImplementationDiscovery.FindImplementation<ICommentService>()!;
        _service = (ICommentService)
            Activator.CreateInstance(
                implType,
                commentRepo,
                postRepo,
                friendshipRepo,
                _profileServiceMock.Object,
                _unitOfWorkMock.Object,
                _notificationRepositoryMock.Object
            )!;
    }

    [ServiceFact(typeof(ICommentService))]
    public async Task CreateAsync_ValidContent_ReturnsComment()
    {
        var request = new CreateCommentRequest(1, "Great post!");
        var result = await _service!.CreateAsync("user1", request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("Great post!");
    }

    [ServiceFact(typeof(ICommentService))]
    public async Task CreateAsync_EmptyContent_ReturnsError()
    {
        var request = new CreateCommentRequest(1, "");
        var result = await _service!.CreateAsync("user1", request);

        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(ICommentService))]
    public async Task CreateAsync_ContentTooLong_ReturnsError()
    {
        var request = new CreateCommentRequest(1, new string('x', 501));
        var result = await _service!.CreateAsync("user1", request);

        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(ICommentService))]
    public async Task CreateReplyAsync_Valid_ReturnsReply()
    {
        var request = new CreateReplyRequest(1, "Nice reply");
        var result = await _service!.CreateReplyAsync("user1", request);

        result.IsSuccess.Should().BeTrue();
        result.Value.ParentCommentId.Should().Be(1);
    }

    [ServiceFact(typeof(ICommentService))]
    public async Task UpdateAsync_NotAuthor_ReturnsError()
    {
        var request = new CreateCommentRequest(1, "Original");
        var created = await _service!.CreateAsync("user1", request);
        if (!created.IsSuccess)
            return;

        var updateReq = new UpdateCommentRequest("Edited");
        var result = await _service!.UpdateAsync("user2", created.Value.Id, updateReq);

        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(ICommentService))]
    public async Task DeleteAsync_NotAuthor_ReturnsError()
    {
        var result = await _service!.DeleteAsync("user2", 1);

        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(ICommentService))]
    public async Task GetPostCommentsAsync_ReturnsList()
    {
        var result = await _service!.GetPostCommentsAsync("user1", 1);

        result.Should().NotBeNull();
    }
}
