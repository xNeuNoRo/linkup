using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Battleship;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Tests.Domain.Entities.Battleship;

public class BattleshipGameTests
{
    [Fact]
    public void Create_ValidPlayers_StartsInCreatorConfigurationPhase()
    {
        // Arrange & Act
        var result = BattleshipGame.Create("creator", "opponent");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(GameStatus.Configuring_P1, result.Value.Status);
        Assert.Null(result.Value.CurrentTurnUserId);
        Assert.Null(result.Value.WinnerId);
    }

    [Fact]
    public void Create_SamePlayer_ReturnsFailure()
    {
        // Arrange & Act
        var result = BattleshipGame.Create("creator", "creator");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, error => error.Code == "Battleship.SelfGameNotAllowed");
    }

    [Fact]
    public void CompletePlayerPlacement_CreatorThenOpponent_StartsAttackPhaseWithCreatorTurn()
    {
        // Arrange
        var game = CreateGameInConfiguringP1();

        // Act
        var creatorResult = game.CompletePlayerPlacement("creator", CreateFleet("creator"));
        var opponentResult = game.CompletePlayerPlacement("opponent", CreateFleet("opponent"));

        // Assert
        Assert.True(creatorResult.IsSuccess);
        Assert.True(opponentResult.IsSuccess);
        Assert.Equal(GameStatus.InProgress, game.Status);
        Assert.Equal("creator", game.CurrentTurnUserId);
        Assert.NotNull(game.TurnAssignedAt);
    }

    [Fact]
    public void CompletePlayerPlacement_InvalidFleetCount_ReturnsFailure()
    {
        // Arrange
        var game = CreateGameInConfiguringP1();
        var incompleteFleet = CreateFleet("creator").Take(4).ToArray();

        // Act
        var result = game.CompletePlayerPlacement("creator", incompleteFleet);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Battleship.InvalidFleetCount", result.Error.Code);
    }

    [Fact]
    public void CompletePlayerPlacement_OverlappingFleet_ReturnsFailure()
    {
        // Arrange
        var game = CreateGameInConfiguringP1();
        var overlappingFleet = new[]
        {
            CreateShip("creator", ShipSize.Size5, 0, 0, ShipDirection.Right),
            CreateShip("creator", ShipSize.Size4, 0, 0, ShipDirection.Down),
            CreateShip("creator", ShipSize.Size3, 0, 2, ShipDirection.Right),
            CreateShip("creator", ShipSize.Size3, 0, 3, ShipDirection.Right),
            CreateShip("creator", ShipSize.Size2, 0, 4, ShipDirection.Right),
        };

        // Act
        var result = game.CompletePlayerPlacement("creator", overlappingFleet);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Battleship.OverlappingShips", result.Error.Code);
    }

    [Fact]
    public void ExecuteAttack_ValidHit_SwitchesTurnToOpponent()
    {
        // Arrange
        var game = CreateGameInProgress();
        var opponentShips = CreateFleet("opponent");

        // Act
        var result = game.ExecuteAttack("creator", Coordinates.Create(0, 0), opponentShips, []);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsHit);
        Assert.Equal("opponent", game.CurrentTurnUserId);
    }

    [Fact]
    public void ExecuteAttack_WhenNotPlayerTurn_ReturnsFailure()
    {
        // Arrange
        var game = CreateGameInProgress();

        // Act
        var result = game.ExecuteAttack("opponent", Coordinates.Create(0, 0), CreateFleet("creator"), []);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Battleship.NotPlayerTurn", result.Error.Code);
    }

    [Fact]
    public void ExecuteAttack_CellAlreadyAttacked_ReturnsFailure()
    {
        // Arrange
        var game = CreateGameInProgress();
        var target = Coordinates.Create(0, 0);
        var existingAttack = BattleshipAttack.Record(0, "creator", target, isHit: true).Value;

        // Act
        var result = game.ExecuteAttack("creator", target, CreateFleet("opponent"), [existingAttack]);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Battleship.CellAlreadyAttacked", result.Error.Code);
    }

    [Fact]
    public void Surrender_PlayerInGame_FinishesGameWithOpponentWinner()
    {
        // Arrange
        var game = CreateGameInProgress();

        // Act
        var result = game.Surrender("creator");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(GameStatus.Finished_Winner, game.Status);
        Assert.Equal("opponent", game.WinnerId);
        Assert.Null(game.CurrentTurnUserId);
    }

    [Fact]
    public void CheckAbandonment_AfterFortyEightHours_FinishesByAbandonment()
    {
        // Arrange
        var now = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var game = CreateGameInProgress(now);

        // Act
        var result = game.CheckAbandonment(now.Add(DomainConstants.BattleshipTurnTimeout));

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(GameStatus.Finished_Abandoned, game.Status);
        Assert.Equal("opponent", game.WinnerId);
    }

    [Fact]
    public void ExecuteAttack_LastRemainingHit_FinishesGameWithAttackerWinner()
    {
        // Arrange
        var game = CreateGameInProgress();
        var opponentShips = CreateFleet("opponent");
        var allOccupiedCells = opponentShips.SelectMany(ship => ship.GetOccupiedCells()).ToArray();
        var lastCell = allOccupiedCells[^1];
        var previousAttacks = allOccupiedCells
            .Take(allOccupiedCells.Length - 1)
            .Select(cell => BattleshipAttack.Record(0, "creator", cell, isHit: true).Value)
            .ToArray();

        // Act
        var result = game.ExecuteAttack("creator", lastCell, opponentShips, previousAttacks);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(GameStatus.Finished_Winner, game.Status);
        Assert.Equal("creator", game.WinnerId);
        Assert.Null(game.CurrentTurnUserId);
    }

    private static BattleshipGame CreateGameInConfiguringP1() => BattleshipGame.Create("creator", "opponent").Value;

    private static BattleshipGame CreateGameInProgress(DateTimeOffset? completedAt = null)
    {
        var game = CreateGameInConfiguringP1();
        game.CompletePlayerPlacement("creator", CreateFleet("creator"), completedAt);
        game.CompletePlayerPlacement("opponent", CreateFleet("opponent"), completedAt);
        return game;
    }

    private static IReadOnlyCollection<BattleshipShip> CreateFleet(string playerId) =>
    [
        CreateShip(playerId, ShipSize.Size5, 0, 0, ShipDirection.Right),
        CreateShip(playerId, ShipSize.Size4, 0, 1, ShipDirection.Right),
        CreateShip(playerId, ShipSize.Size3, 0, 2, ShipDirection.Right),
        CreateShip(playerId, ShipSize.Size3, 0, 3, ShipDirection.Right),
        CreateShip(playerId, ShipSize.Size2, 0, 4, ShipDirection.Right),
    ];

    private static BattleshipShip CreateShip(string playerId, ShipSize size, int x, int y, ShipDirection direction) =>
        BattleshipShip.Place(0, playerId, ShipPlacement.Create(Coordinates.Create(x, y), direction, size)).Value;
}
