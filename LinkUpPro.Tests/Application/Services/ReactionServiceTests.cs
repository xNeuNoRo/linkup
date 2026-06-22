using LinkUpPro.Application.DTOs.Reaction.Requests;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Tests.Base;
using Moq;

namespace LinkUpPro.Tests.Application.Services;

[Collection("Sequential")]
public class ReactionServiceTests : InMemoryTestBase
{
    private IReactionService? _service;
    private bool _hasImplementation;

    public ReactionServiceTests()
    {
        _hasImplementation = ImplementationDiscovery.HasImplementation<IReactionService>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (!_hasImplementation) return;

        var implType = ImplementationDiscovery.FindImplementation<IReactionService>()!;
        _service = (IReactionService)Activator.CreateInstance(implType,
            null!, null!, null!)!;
    }

    [ServiceFact(typeof(IReactionService))]
    public async Task ReactAsync_Like_ReturnsReaction()
    {
        var req = new CreateReactionRequest(1, 1);
        var result = await _service!.ReactAsync("user1", req);
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(1);
    }

    [ServiceFact(typeof(IReactionService))]
    public async Task ReactAsync_Dislike_ReturnsReaction()
    {
        var req = new CreateReactionRequest(1, 0);
        var result = await _service!.ReactAsync("user1", req);
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(0);
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
        var req = new CreateReactionRequest(1, 1);
        await _service!.ReactAsync("user1", req);

        var result = await _service!.GetUserReactionAsync("user1", 1);
        result.Should().NotBeNull();
    }

    [ServiceFact(typeof(IReactionService))]
    public async Task GetUserReactionAsync_NoReaction_ReturnsNull()
    {
        var result = await _service!.GetUserReactionAsync("user1", 999);
        result.Should().BeNull();
    }
}
