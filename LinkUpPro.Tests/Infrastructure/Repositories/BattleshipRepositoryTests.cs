using LinkUpPro.Domain.Entities.Battleship;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.ValueObjects;
using LinkUpPro.Infrastructure.Persistence.Repositories;

namespace LinkUpPro.Tests.Infrastructure.Repositories;

public sealed class BattleshipRepositoryTests : PersistenceTestBase
{
    [Fact]
    public async Task GetActiveGamesForUserAsync_ReturnsActiveGames()
    {
        var context = CreateContext();
        var repo = new BattleshipRepository(context);
        var creatorId = CreateUserId("creator");
        var opponentId = CreateUserId("opponent");

        var game = BattleshipGame.Create(creatorId, opponentId).Value;
        context.BattleshipGames.Add(game);
        await context.SaveChangesAsync();

        var result = await repo.GetActiveGamesForUserAsync(creatorId);

        result.Should().HaveCount(1);
        result.First().Status.Should().Be(GameStatus.Configuring_P1);
    }

    [Fact]
    public async Task GetFinishedGamesForUserAsync_ReturnsFinishedGames()
    {
        var context = CreateContext();
        var repo = new BattleshipRepository(context);
        var creatorId = CreateUserId("creator");
        var opponentId = CreateUserId("opponent");

        var game = BattleshipGame.Create(creatorId, opponentId).Value;
        context.BattleshipGames.Add(game);
        await context.SaveChangesAsync();

        game.CompletePlayerPlacement(
            creatorId,
            new List<BattleshipShip>
            {
                CreateShip(game.Id, creatorId, ShipSize.Size5),
                CreateShip(game.Id, creatorId, ShipSize.Size4),
                CreateShip(game.Id, creatorId, ShipSize.Size3),
                CreateShip(game.Id, creatorId, ShipSize.Size3),
                CreateShip(game.Id, creatorId, ShipSize.Size2),
            }
        );

        game.CompletePlayerPlacement(
            opponentId,
            new List<BattleshipShip>
            {
                CreateShip(game.Id, opponentId, ShipSize.Size5),
                CreateShip(game.Id, opponentId, ShipSize.Size4),
                CreateShip(game.Id, opponentId, ShipSize.Size3),
                CreateShip(game.Id, opponentId, ShipSize.Size3),
                CreateShip(game.Id, opponentId, ShipSize.Size2),
            }
        );

        game.Surrender(opponentId);
        await context.SaveChangesAsync();

        var result = await repo.GetFinishedGamesForUserAsync(creatorId);

        result.Should().HaveCount(1);
        result.First().Status.Should().Be(GameStatus.Finished_Winner);
    }

    [Fact]
    public async Task HasActiveGameBetweenAsync_Existing_ReturnsTrue()
    {
        var context = CreateContext();
        var repo = new BattleshipRepository(context);
        var creatorId = CreateUserId("creator");
        var opponentId = CreateUserId("opponent");

        context.BattleshipGames.Add(BattleshipGame.Create(creatorId, opponentId).Value);
        await context.SaveChangesAsync();

        var result = await repo.HasActiveGameBetweenAsync(creatorId, opponentId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetStatsForUserAsync_ReturnsCorrectStats()
    {
        var context = CreateContext();
        var repo = new BattleshipRepository(context);
        var creatorId = CreateUserId("creator");
        var opponentId = CreateUserId("opponent");

        var game = BattleshipGame.Create(creatorId, opponentId).Value;
        context.BattleshipGames.Add(game);
        await context.SaveChangesAsync();

        game.Surrender(opponentId);
        await context.SaveChangesAsync();

        var stats = await repo.GetStatsForUserAsync(creatorId);

        stats.TotalGames.Should().Be(1);
        stats.WonGames.Should().Be(1);
        stats.LostGames.Should().Be(0);
    }

    [Fact]
    public async Task GetShipsByGameAndPlayerAsync_ReturnsCorrectShips()
    {
        var context = CreateContext();
        var repo = new BattleshipRepository(context);
        var creatorId = CreateUserId("creator");
        var opponentId = CreateUserId("opponent");

        var game = BattleshipGame.Create(creatorId, opponentId).Value;
        context.BattleshipGames.Add(game);
        await context.SaveChangesAsync();

        var ship = CreateShip(game.Id, creatorId, ShipSize.Size5);
        context.BattleshipShips.Add(ship);
        await context.SaveChangesAsync();

        var result = await repo.GetShipsByGameAndPlayerAsync(game.Id, creatorId);

        result.Should().HaveCount(1);
        result.First().Size.Should().Be(ShipSize.Size5);
    }

    [Fact]
    public async Task GetAttacksByGameAsync_ReturnsGameAttacks()
    {
        var context = CreateContext();
        var repo = new BattleshipRepository(context);
        var creatorId = CreateUserId("creator");
        var opponentId = CreateUserId("opponent");

        var game = BattleshipGame.Create(creatorId, opponentId).Value;
        context.BattleshipGames.Add(game);
        await context.SaveChangesAsync();

        var attack = BattleshipAttack
            .Record(game.Id, creatorId, Coordinates.Create(0, 0), isHit: false)
            .Value;
        context.BattleshipAttacks.Add(attack);
        await context.SaveChangesAsync();

        var result = await repo.GetAttacksByGameAsync(game.Id);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetGameWithShipsAndAttacksAsync_IncludesAllData()
    {
        var context = CreateContext();
        var repo = new BattleshipRepository(context);
        var creatorId = CreateUserId("creator");
        var opponentId = CreateUserId("opponent");

        var game = BattleshipGame.Create(creatorId, opponentId).Value;
        context.BattleshipGames.Add(game);
        await context.SaveChangesAsync();

        context.BattleshipShips.Add(CreateShip(game.Id, creatorId, ShipSize.Size5));
        context.BattleshipAttacks.Add(
            BattleshipAttack.Record(game.Id, creatorId, Coordinates.Create(0, 0), false).Value
        );
        await context.SaveChangesAsync();

        var result = await repo.GetGameWithShipsAndAttacksAsync(game.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(game.Id);
    }

    private static BattleshipShip CreateShip(long gameId, string playerId, ShipSize size)
    {
        var startX = size switch
        {
            ShipSize.Size5 => 0,
            ShipSize.Size4 => 2,
            ShipSize.Size3 => 4,
            ShipSize.Size2 => 8,
            _ => 10,
        };

        var placement = ShipPlacement.Create(
            Coordinates.Create((byte)startX, 0),
            ShipDirection.Right,
            size
        );

        return BattleshipShip.Place(gameId, playerId, placement).Value;
    }
}
