using LinkUpPro.Application.DTOs.Battleship.Requests;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Tests.Base;
using Moq;

namespace LinkUpPro.Tests.Application.Services;

[Collection("Sequential")]
public class BattleshipServiceTests : InMemoryTestBase
{
    private IBattleshipService? _service;
    private bool _hasImplementation;

    public BattleshipServiceTests()
    {
        _hasImplementation = ImplementationDiscovery.HasImplementation<IBattleshipService>();
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        if (!_hasImplementation) return;

        var implType = ImplementationDiscovery.FindImplementation<IBattleshipService>()!;
        _service = (IBattleshipService)Activator.CreateInstance(implType,
            null!, null!, null!)!;
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task CreateGameAsync_WithFriend_CreatesGame()
    {
        var req = new CreateGameRequest("user2");
        var result = await _service!.CreateGameAsync("user1", req);
        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task CreateGameAsync_WithActiveGame_ReturnsError()
    {
        var req = new CreateGameRequest("user2");
        await _service!.CreateGameAsync("user1", req);
        var second = await _service!.CreateGameAsync("user1", req);
        second.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task PlaceShipAsync_Valid_PlacesShip()
    {
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess) return;

        var shipReq = new PlaceShipRequest(3, 0, 0, 2);
        var result = await _service!.PlaceShipAsync("user1", game.Value.Id, shipReq);
        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task PlaceShipAsync_OutOfBounds_ReturnsError()
    {
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess) return;

        var shipReq = new PlaceShipRequest(5, 10, 10, 0);
        var result = await _service!.PlaceShipAsync("user1", game.Value.Id, shipReq);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task PlaceShipAsync_Overlap_ReturnsError()
    {
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess) return;

        await _service!.PlaceShipAsync("user1", game.Value.Id, new PlaceShipRequest(4, 0, 0, 2));
        var second = await _service!.PlaceShipAsync("user1", game.Value.Id, new PlaceShipRequest(3, 0, 1, 2));
        second.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task AttackAsync_Hit_MarksRed()
    {
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess) return;

        await _service!.PlaceShipAsync("user1", game.Value.Id, new PlaceShipRequest(3, 0, 0, 2));
        await _service!.PlaceShipAsync("user2", game.Value.Id, new PlaceShipRequest(3, 5, 5, 2));

        var attackReq = new AttackRequest(5, 5);
        var result = await _service!.AttackAsync("user1", game.Value.Id, attackReq);
        result.IsSuccess.Should().BeTrue();
        result.Value.IsHit.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task AttackAsync_Miss_MarksGreen()
    {
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess) return;

        var attackReq = new AttackRequest(0, 0);
        var result = await _service!.AttackAsync("user1", game.Value.Id, attackReq);
        result.IsSuccess.Should().BeTrue();
        result.Value.IsHit.Should().BeFalse();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task AttackAsync_NotYourTurn_ReturnsError()
    {
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess) return;

        var attackReq = new AttackRequest(5, 5);
        var result = await _service!.AttackAsync("user2", game.Value.Id, attackReq);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task AttackAsync_AlreadyAttacked_ReturnsError()
    {
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess) return;

        await _service!.AttackAsync("user1", game.Value.Id, new AttackRequest(5, 5));
        var second = await _service!.AttackAsync("user1", game.Value.Id, new AttackRequest(5, 5));
        second.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task SurrenderAsync_Valid_GivesOpponentWin()
    {
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess) return;

        var surrenderReq = new SurrenderRequest(game.Value.Id);
        var result = await _service!.SurrenderAsync("user1", surrenderReq);
        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task SurrenderAsync_AlreadyFinished_ReturnsError()
    {
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess) return;

        await _service!.SurrenderAsync("user1", new SurrenderRequest(game.Value.Id));
        var second = await _service!.SurrenderAsync("user1", new SurrenderRequest(game.Value.Id));
        second.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task SurrenderAsync_NotParticipant_ReturnsError()
    {
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess) return;

        var result = await _service!.SurrenderAsync("user3", new SurrenderRequest(game.Value.Id));
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task GetStatsAsync_ReturnsStats()
    {
        var result = await _service!.GetStatsAsync("user1");
        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task GetActiveGamesAsync_ReturnsActive()
    {
        var result = await _service!.GetActiveGamesAsync("user1", 1, 20);
        result.Should().NotBeNull();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task GetGameHistoryAsync_ReturnsHistory()
    {
        var result = await _service!.GetGameHistoryAsync("user1", 1, 20);
        result.Should().NotBeNull();
    }
}
