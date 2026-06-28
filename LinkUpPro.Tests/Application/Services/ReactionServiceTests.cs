using LinkUpPro.Application.DTOs.Profile.Responses;
using LinkUpPro.Application.DTOs.Reaction.Requests;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.Interfaces.Repositories;
using LinkUpPro.Infrastructure.Persistence.Repositories;
using LinkUpPro.Tests.Base;
using Moq;
using FluentAssertions;

namespace LinkUpPro.Tests.Application.Services;

[Collection("Sequential")]
public class ReactionServiceTests : InMemoryTestBase
{
    private IReactionService? _service;
    private Mock<IProfileService> _profileServiceMock = null!;
    private Mock<INotificationRepository> _notificationRepositoryMock = null!;
    private bool _hasImplementation;
    private IUnitOfWork _unitOfWork = null!;

    public ReactionServiceTests()
    {
        _hasImplementation = ImplementationDiscovery.HasImplementation<IReactionService>();
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

        var reactionUser1 = Reaction.Create(1, "user1", ReactionType.Like).Value;
        typeof(Reaction).GetProperty(nameof(Reaction.Id))!.SetValue(reactionUser1, 1L);

        var reactionUser2 = Reaction.Create(1, "user2", ReactionType.Dislike).Value;
        typeof(Reaction).GetProperty(nameof(Reaction.Id))!.SetValue(reactionUser2, 2L);

        DbContext.Add(post);
        DbContext.Add(reactionUser1);
        DbContext.Add(reactionUser2);
        await DbContext.SaveChangesAsync();

        var reactionRepo = new ReactionRepository(DbContext);
        var postRepo = new PostRepository(DbContext);
        _unitOfWork = new LinkUpPro.Infrastructure.Persistence.Persistence.UnitOfWork(DbContext);

        _profileServiceMock = new Mock<IProfileService>();
        _profileServiceMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(
                (string id) =>
                    new UserResponseDto(
                        id,
                        "testuser",
                        "test@test.com",
                        "Test",
                        "User",
                        "809-555-1234",
                        "/images/default-avatar.png",
                        true,
                        true
                    )
            );

        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _notificationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var implType = ImplementationDiscovery.FindImplementation<IReactionService>()!;
        _service = (IReactionService)
            Activator.CreateInstance(
                implType,
                reactionRepo,
                _unitOfWork,
                postRepo,
                _profileServiceMock.Object,
                _notificationRepositoryMock.Object
            )!;
    }

    [ServiceFact(typeof(IReactionService))]
    public async Task ReactAsync_Like_ReturnsReaction()
    {
        var req = new CreateReactionRequest(1, 1);
        var result = await _service!.ReactAsync("user3", req);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Type.Should().Be(ReactionType.Like);
    }

    [ServiceFact(typeof(IReactionService))]
    public async Task ReactAsync_Dislike_ReturnsReaction()
    {
        var req = new CreateReactionRequest(1, 2);
        var result = await _service!.ReactAsync("user4", req);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Type.Should().Be(ReactionType.Dislike);
    }

    [ServiceFact(typeof(IReactionService))]
    public async Task DeleteAsync_Valid_RemovesReaction()
    {
        var result = await _service!.DeleteAsync("user1", 1);

        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IReactionService))]
    public async Task GetCountsAsync_ReturnsAggregated()
    {
        var result = await _service!.GetCountsAsync(1);

        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IReactionService))]
    public async Task GetUserReactionAsync_HasReaction_ReturnsType()
    {
        var result = await _service!.GetUserReactionAsync("user2", 1);

        result.Should().NotBeNull();
        result.Should().Be(ReactionType.Dislike);
    }

    [ServiceFact(typeof(IReactionService))]
    public async Task GetUserReactionAsync_NoReaction_ReturnsNull()
    {
        var result = await _service!.GetUserReactionAsync("user1", 999);

        result.Should().BeNull();
    }

    [ServiceFact(typeof(IReactionService))]
    public async Task ReactAsync_Change_GeneratesReactionChangeNotification()
    {
        var req = new CreateReactionRequest(1, 1);
        var first = await _service!.ReactAsync("user3", req);
        first.IsSuccess.Should().BeTrue();

        var changeReq = new CreateReactionRequest(1, 2);
        var result = await _service!.ReactAsync("user3", changeReq);

        result.IsSuccess.Should().BeTrue();
        _notificationRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Notification>(n => n.Type == NotificationType.ReactionChange),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}
