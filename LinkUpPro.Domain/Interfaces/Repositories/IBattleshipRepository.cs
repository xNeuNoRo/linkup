using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Battleship;

namespace LinkUpPro.Domain.Interfaces.Repositories;

public interface IBattleshipRepository : IGenericRepository<BattleshipGame, long>
{
    Task<IReadOnlyCollection<BattleshipGame>> GetActiveGamesForUserAsync(
        string userId,
        QueryOptions<BattleshipGame>? options = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<BattleshipGame>> GetFinishedGamesForUserAsync(
        string userId,
        QueryOptions<BattleshipGame>? options = null,
        CancellationToken cancellationToken = default
    );

    Task<BattleshipGame?> GetGameWithShipsAndAttacksAsync(
        long gameId,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasActiveGameBetweenAsync(
        string firstUserId,
        string secondUserId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<BattleshipShip>> GetShipsByGameAndPlayerAsync(
        long gameId,
        string playerId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<BattleshipShip>> GetShipsByGameAsync(
        long gameId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<BattleshipAttack>> GetAttacksByGameAsync(
        long gameId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<BattleshipAttack>> GetAttacksByGameAndAttackerAsync(
        long gameId,
        string attackerId,
        CancellationToken cancellationToken = default
    );

    Task<BattleshipGameStats> GetStatsForUserAsync(
        string userId,
        CancellationToken cancellationToken = default
    );
}
