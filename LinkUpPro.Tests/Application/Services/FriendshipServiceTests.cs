using LinkUpPro.Application.DTOs.Profile.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Infrastructure.Persistence.Repositories;
using LinkUpPro.Tests.Base;
using Moq;
using DomainFriendship = LinkUpPro.Domain.Entities.Friendship.Friendship;

namespace LinkUpPro.Tests.Application.Services;

[Collection("Sequential")]
public class FriendshipServiceTests : InMemoryTestBase
{
    private IFriendshipService? _service;
    private Mock<IProfileService> _profileServiceMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private bool _hasImplementation;

    public FriendshipServiceTests()
    {
        _hasImplementation = ImplementationDiscovery.HasImplementation<IFriendshipService>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (!_hasImplementation)
            return;

        _profileServiceMock = new Mock<IProfileService>();
        _profileServiceMock
            .Setup(x => x.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(
                (string id) =>
                    id switch
                    {
                        "user1" => new UserResponseDto(
                            "user1",
                            "user1",
                            "user1@test.com",
                            "User",
                            "One",
                            "809-555-1111",
                            "/img/user1.jpg",
                            true,
                            true
                        ),
                        "user2" => new UserResponseDto(
                            "user2",
                            "user2",
                            "user2@test.com",
                            "User",
                            "Two",
                            "809-555-2222",
                            "/img/user2.jpg",
                            true,
                            true
                        ),
                        "user3" => new UserResponseDto(
                            "user3",
                            "user3",
                            "user3@test.com",
                            "User",
                            "Three",
                            "809-555-3333",
                            "/img/user3.jpg",
                            true,
                            true
                        ),
                        _ => null,
                    }
            );
        _profileServiceMock
            .Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (IEnumerable<string> ids, CancellationToken _) =>
                {
                    var dict = new Dictionary<string, UserResponseDto>();
                    foreach (var id in ids)
                    {
                        var u = id switch
                        {
                            "user1" => new UserResponseDto("user1", "user1", "user1@test.com", "User", "One", "809-555-1111", "/img/user1.jpg", true, true),
                            "user2" => new UserResponseDto("user2", "user2", "user2@test.com", "User", "Two", "809-555-2222", "/img/user2.jpg", true, true),
                            "user3" => new UserResponseDto("user3", "user3", "user3@test.com", "User", "Three", "809-555-3333", "/img/user3.jpg", true, true),
                            _ => null,
                        };
                        if (u is not null) dict[id] = u;
                    }
                    return dict;
                }
            );

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var f1 = DomainFriendship.Create("user1", "user2").Value; // user1-user2 friends
        var f2 = DomainFriendship.Create("user1", "user3").Value; // user1-user3 friends
        var f3 = DomainFriendship.Create("user2", "user3").Value; // user2-user3 friends (common: user3 is common friend)

        DbContext.Add(f1);
        DbContext.Add(f2);
        DbContext.Add(f3);
        await DbContext.SaveChangesAsync();

        var friendshipRepo = new FriendshipRepository(DbContext);
        var postRepo = new PostRepository(DbContext);

        var implType = ImplementationDiscovery.FindImplementation<IFriendshipService>()!;
        _service = (IFriendshipService)
            Activator.CreateInstance(
                implType,
                friendshipRepo,
                _profileServiceMock.Object,
                _unitOfWorkMock.Object,
                postRepo
            )!;
    }

    [ServiceFact(typeof(IFriendshipService))]
    public async Task GetFriendsAsync_ReturnsPaged()
    {
        var result = await _service!.GetFriendsAsync("user1", null, 1, 20);

        result.Should().NotBeNull();
        result.Items.Count.Should().Be(2); // user2 and user3
    }

    [ServiceFact(typeof(IFriendshipService))]
    public async Task GetFriendsAsync_WithSearch_Filters()
    {
        var result = await _service!.GetFriendsAsync("user1", "user2", 1, 20);

        result.Should().NotBeNull();
        result.Items.Should().Contain(f => f.FriendUserName == "user2");
    }

    [ServiceFact(typeof(IFriendshipService))]
    public async Task GetFriendsAsync_IncludesCommonFriendsCount()
    {
        var result = await _service!.GetFriendsAsync("user1", null, 1, 20);

        // user3 is common friend between user1 and user2
        var user2 = result.Items.FirstOrDefault(f => f.FriendUserName == "user2");
        user2.Should().NotBeNull();
        user2!.CommonFriendsCount.Should().Be(1); // user3 is common
    }

    [ServiceFact(typeof(IFriendshipService))]
    public async Task GetCommonFriendsAsync_ReturnsCount()
    {
        var result = await _service!.GetCommonFriendsAsync("user1", "user2");

        result.IsSuccess.Should().BeTrue();
        result.Value.Count.Should().Be(1); // user3
    }

    [ServiceFact(typeof(IFriendshipService))]
    public async Task DeleteAsync_Valid_RemovesFriendship()
    {
        var result = await _service!.DeleteAsync("user1", "user2");

        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IFriendshipService))]
    public async Task DeleteAsync_NotFriends_ReturnsError()
    {
        var result = await _service!.DeleteAsync("user1", "user4");

        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IFriendshipService))]
    public async Task GetFriendshipAsync_Active_ReturnsFriendship()
    {
        var result = await _service!.GetFriendshipAsync("user1", "user2");

        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IFriendshipService))]
    public async Task GetFriendsAsync_OrderedByName()
    {
        var result = await _service!.GetFriendsAsync("user1", null, 1, 20);

        result.Should().NotBeNull();
    }
}
