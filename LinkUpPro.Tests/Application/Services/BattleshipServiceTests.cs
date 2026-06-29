using LinkUpPro.Application.DTOs.Battleship.Requests;
using LinkUpPro.Application.DTOs.Profile.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Infrastructure.Persistence.Repositories;
using LinkUpPro.Tests.Base;
using Moq;

namespace LinkUpPro.Tests.Application.Services;

[Collection("Sequential")]
public class BattleshipServiceTests : InMemoryTestBase
{
    private IBattleshipService? _service;
    private Mock<IProfileService> _profileServiceMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private bool _hasImplementation;

    public BattleshipServiceTests()
    {
        _hasImplementation = ImplementationDiscovery.HasImplementation<IBattleshipService>();
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
                    new UserResponseDto(
                        id,
                        id,
                        $"{id}@test.com",
                        "User",
                        id,
                        "809-555-0000",
                        "/img/default.jpg",
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
                            id, id, $"{id}@test.com", "User", id, "809-555-0000",
                            "/img/default.jpg", true, true)
                    )
            );

        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var battleshipRepository = new BattleshipRepository(DbContext);
        var friendshipRepository = new FriendshipRepository(DbContext);
        var unitOfWork = new LinkUpPro.Infrastructure.Persistence.Persistence.UnitOfWork(DbContext);

        var implType = ImplementationDiscovery.FindImplementation<IBattleshipService>()!;
        _service = (IBattleshipService)
            Activator.CreateInstance(
                implType,
                battleshipRepository,
                friendshipRepository,
                _profileServiceMock.Object,
                unitOfWork
            )!;
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task CreateGameAsync_WithFriend_CreatesGame()
    {
        SeedFriendship("user1", "user2");
        var req = new CreateGameRequest("user2");
        var result = await _service!.CreateGameAsync("user1", req);
        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task CreateGameAsync_WithActiveGame_ReturnsError()
    {
        SeedFriendship("user1", "user2");
        var req = new CreateGameRequest("user2");
        await _service!.CreateGameAsync("user1", req);
        var second = await _service!.CreateGameAsync("user1", req);
        second.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task PlaceShipAsync_Valid_PlacesShip()
    {
        SeedFriendship("user1", "user2");
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess)
            return;

        var shipReq = new PlaceShipRequest(3, 0, 0, 2);
        var result = await _service!.PlaceShipAsync("user1", game.Value.Id, shipReq);
        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task PlaceShipAsync_OutOfBounds_ReturnsError()
    {
        SeedFriendship("user1", "user2");
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess)
            return;

        var shipReq = new PlaceShipRequest(5, 10, 10, 0);
        var result = await _service!.PlaceShipAsync("user1", game.Value.Id, shipReq);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task PlaceShipAsync_Overlap_ReturnsError()
    {
        SeedFriendship("user1", "user2");
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess)
            return;

        await _service!.PlaceShipAsync("user1", game.Value.Id, new PlaceShipRequest(4, 0, 0, 2));
        var second = await _service!.PlaceShipAsync(
            "user1",
            game.Value.Id,
            new PlaceShipRequest(3, 0, 1, 2)
        );
        second.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task AttackAsync_Hit_MarksRed()
    {
        SeedFriendship("user1", "user2");
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess)
            return;

        // Place 5 ships for user1
        await _service!.PlaceShipAsync("user1", game.Value.Id, new PlaceShipRequest(2, 0, 0, 2));
        await _service!.PlaceShipAsync("user1", game.Value.Id, new PlaceShipRequest(3, 0, 2, 2));
        await _service!.PlaceShipAsync("user1", game.Value.Id, new PlaceShipRequest(3, 0, 5, 2));
        await _service!.PlaceShipAsync("user1", game.Value.Id, new PlaceShipRequest(4, 0, 8, 2));
        await _service!.PlaceShipAsync("user1", game.Value.Id, new PlaceShipRequest(5, 5, 0, 4));
        // Place 5 ships for user2 (no overlap with each other, includes (5,5) for the hit)
        await _service!.PlaceShipAsync("user2", game.Value.Id, new PlaceShipRequest(2, 5, 5, 2));
        await _service!.PlaceShipAsync("user2", game.Value.Id, new PlaceShipRequest(3, 5, 7, 2));
        await _service!.PlaceShipAsync("user2", game.Value.Id, new PlaceShipRequest(3, 7, 0, 4));
        await _service!.PlaceShipAsync("user2", game.Value.Id, new PlaceShipRequest(4, 10, 0, 2));
        await _service!.PlaceShipAsync("user2", game.Value.Id, new PlaceShipRequest(5, 6, 0, 2));

        var attackReq = new AttackRequest(5, 5);
        var result = await _service!.AttackAsync("user1", game.Value.Id, attackReq);
        if (!result.IsSuccess)
            Console.WriteLine(
                $"Attack failed: {string.Join(",", result.Errors.Select(e => $"{e.Code}: {e.Message}"))}"
            );
        result.IsSuccess.Should().BeTrue();
        result.Value.IsHit.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task AttackAsync_Miss_MarksGreen()
    {
        SeedFriendship("user1", "user2");
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess)
            return;

        // Place 5 ships for user1
        await _service!.PlaceShipAsync("user1", game.Value.Id, new PlaceShipRequest(2, 0, 0, 2));
        await _service!.PlaceShipAsync("user1", game.Value.Id, new PlaceShipRequest(3, 0, 2, 2));
        await _service!.PlaceShipAsync("user1", game.Value.Id, new PlaceShipRequest(3, 0, 5, 2));
        await _service!.PlaceShipAsync("user1", game.Value.Id, new PlaceShipRequest(4, 0, 8, 2));
        await _service!.PlaceShipAsync("user1", game.Value.Id, new PlaceShipRequest(5, 5, 0, 4));
        // Place 5 ships for user2 (no overlap)
        await _service!.PlaceShipAsync("user2", game.Value.Id, new PlaceShipRequest(2, 5, 5, 2));
        await _service!.PlaceShipAsync("user2", game.Value.Id, new PlaceShipRequest(3, 5, 7, 2));
        await _service!.PlaceShipAsync("user2", game.Value.Id, new PlaceShipRequest(3, 7, 0, 4));
        await _service!.PlaceShipAsync("user2", game.Value.Id, new PlaceShipRequest(4, 10, 0, 2));
        await _service!.PlaceShipAsync("user2", game.Value.Id, new PlaceShipRequest(5, 6, 0, 2));

        // Attack at (0,0) which is NOT in user2's ships = miss
        var attackReq = new AttackRequest(0, 0);
        var result = await _service!.AttackAsync("user1", game.Value.Id, attackReq);
        if (!result.IsSuccess)
            Console.WriteLine(
                $"Attack failed: {string.Join(",", result.Errors.Select(e => $"{e.Code}: {e.Message}"))}"
            );
        result.IsSuccess.Should().BeTrue();
        result.Value.IsHit.Should().BeFalse();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task AttackAsync_NotYourTurn_ReturnsError()
    {
        SeedFriendship("user1", "user2");
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess)
            return;

        var attackReq = new AttackRequest(5, 5);
        var result = await _service!.AttackAsync("user2", game.Value.Id, attackReq);
        result.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task AttackAsync_AlreadyAttacked_ReturnsError()
    {
        SeedFriendship("user1", "user2");
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess)
            return;

        await _service!.AttackAsync("user1", game.Value.Id, new AttackRequest(5, 5));
        var second = await _service!.AttackAsync("user1", game.Value.Id, new AttackRequest(5, 5));
        second.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task SurrenderAsync_Valid_GivesOpponentWin()
    {
        SeedFriendship("user1", "user2");
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess)
            return;

        var surrenderReq = new SurrenderRequest(game.Value.Id);
        var result = await _service!.SurrenderAsync("user1", surrenderReq);
        result.IsSuccess.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task SurrenderAsync_AlreadyFinished_ReturnsError()
    {
        SeedFriendship("user1", "user2");
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess)
            return;

        await _service!.SurrenderAsync("user1", new SurrenderRequest(game.Value.Id));
        var second = await _service!.SurrenderAsync("user1", new SurrenderRequest(game.Value.Id));
        second.IsFailure.Should().BeTrue();
    }

    [ServiceFact(typeof(IBattleshipService))]
    public async Task SurrenderAsync_NotParticipant_ReturnsError()
    {
        SeedFriendship("user1", "user2");
        var req = new CreateGameRequest("user2");
        var game = await _service!.CreateGameAsync("user1", req);
        if (!game.IsSuccess)
            return;

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

    private void SeedFriendship(string user1, string user2)
    {
        var friendship = LinkUpPro.Domain.Entities.Friendship.Friendship.Create(user1, user2).Value;
        DbContext.Friendships.Add(friendship);
        DbContext.SaveChanges();
    }
}
