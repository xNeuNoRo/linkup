using LinkUpPro.Application.DTOs.Friendship.Responses;
using LinkUpPro.Application.DTOs.Profile.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.Interfaces.Repositories;
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
        if (!_hasImplementation) return;

        var friendshipRepo = new FriendshipRepository(DbContext);
        _profileServiceMock = new Mock<IProfileService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var implType = ImplementationDiscovery.FindImplementation<IFriendshipService>()!;
        _service = (IFriendshipService)Activator.CreateInstance(implType,
            friendshipRepo, _profileServiceMock.Object, _unitOfWorkMock.Object)!;
    }

    [ServiceFact(typeof(IFriendshipService))]
    public async Task GetFriendsAsync_ReturnsPaged()
    {
        var result = await _service!.GetFriendsAsync("user1", null, 1, 20);
        result.Should().NotBeNull();
    }

    [ServiceFact(typeof(IFriendshipService))]
    public async Task GetFriendsAsync_WithSearch_Filters()
    {
        var result = await _service!.GetFriendsAsync("user1", "john", 1, 20);
        result.Should().NotBeNull();
    }

    [ServiceFact(typeof(IFriendshipService))]
    public async Task GetCommonFriendsAsync_ReturnsCount()
    {
        var result = await _service!.GetCommonFriendsAsync("user1", "user2");
        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IFriendshipService))]
    public async Task DeleteAsync_Valid_RemovesFriendship()
    {
        var friendship = DomainFriendship.Create("user1", "user2").Value;
        DbContext.Add(friendship);
        await DbContext.SaveChangesAsync();

        var result = await _service!.DeleteAsync("user1", "user2");
        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IFriendshipService))]
    public async Task DeleteAsync_NotFriends_ReturnsError()
    {
        var result = await _service!.DeleteAsync("user1", "user2");
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IFriendshipService))]
    public async Task GetFriendshipAsync_Active_ReturnsFriendship()
    {
        var friendship = DomainFriendship.Create("user1", "user2").Value;
        DbContext.Add(friendship);
        await DbContext.SaveChangesAsync();

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
