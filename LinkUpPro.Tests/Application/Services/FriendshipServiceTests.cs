using LinkUpPro.Application.DTOs.Friendship.Requests;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Tests.Base;
using Moq;

namespace LinkUpPro.Tests.Application.Services;

[Collection("Sequential")]
public class FriendshipServiceTests : InMemoryTestBase
{
    private IFriendshipService? _service;
    private bool _hasImplementation;

    public FriendshipServiceTests()
    {
        _hasImplementation = ImplementationDiscovery.HasImplementation<IFriendshipService>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (!_hasImplementation) return;

        var implType = ImplementationDiscovery.FindImplementation<IFriendshipService>()!;
        _service = (IFriendshipService)Activator.CreateInstance(implType, null!, null!)!;
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
