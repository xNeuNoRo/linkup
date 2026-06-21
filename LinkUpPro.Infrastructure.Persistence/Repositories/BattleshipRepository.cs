using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Battleship;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.Interfaces.Repositories;
using LinkUpPro.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LinkUpPro.Infrastructure.Persistence.Repositories;

public sealed class BattleshipRepository
    : GenericRepository<BattleshipGame, long>,
        IBattleshipRepository
{
    public BattleshipRepository(AppDbContext context)
        : base(context) { }

    public async Task<IReadOnlyCollection<BattleshipGame>> GetActiveGamesForUserAsync(
        string userId,
        QueryOptions<BattleshipGame>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(g =>
            (g.CreatorId == userId || g.OpponentId == userId)
            && g.Status != GameStatus.Finished_Winner
            && g.Status != GameStatus.Finished_Abandoned
        );
        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<BattleshipGame>> GetFinishedGamesForUserAsync(
        string userId,
        QueryOptions<BattleshipGame>? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbSet.Where(g =>
            (g.CreatorId == userId || g.OpponentId == userId)
            && (g.Status == GameStatus.Finished_Winner || g.Status == GameStatus.Finished_Abandoned)
        );
        return await ApplyOptionsToQuery(query, options).ToListAsync(cancellationToken);
    }

    public async Task<BattleshipGame?> GetGameWithShipsAndAttacksAsync(
        long gameId,
        CancellationToken cancellationToken = default
    )
    {
        var ships = await _context
            .Set<BattleshipShip>()
            .Where(s => s.GameId == gameId)
            .ToListAsync(cancellationToken);

        var attacks = await _context
            .Set<BattleshipAttack>()
            .Where(a => a.GameId == gameId)
            .ToListAsync(cancellationToken);

        var game = await _dbSet.FirstOrDefaultAsync(g => g.Id == gameId, cancellationToken);

        return game;
    }

    public async Task<bool> HasActiveGameBetweenAsync(
        string firstUserId,
        string secondUserId,
        CancellationToken cancellationToken = default
    )
    {
        return await _dbSet.AnyAsync(
            g =>
                (
                    (g.CreatorId == firstUserId && g.OpponentId == secondUserId)
                    || (g.CreatorId == secondUserId && g.OpponentId == firstUserId)
                )
                && g.Status != GameStatus.Finished_Winner
                && g.Status != GameStatus.Finished_Abandoned,
            cancellationToken
        );
    }

    public async Task<IReadOnlyCollection<BattleshipShip>> GetShipsByGameAndPlayerAsync(
        long gameId,
        string playerId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Set<BattleshipShip>()
            .Where(s => s.GameId == gameId && s.PlayerId == playerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<BattleshipShip>> GetShipsByGameAsync(
        long gameId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Set<BattleshipShip>()
            .Where(s => s.GameId == gameId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<BattleshipAttack>> GetAttacksByGameAsync(
        long gameId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Set<BattleshipAttack>()
            .Where(a => a.GameId == gameId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<BattleshipAttack>> GetAttacksByGameAndAttackerAsync(
        long gameId,
        string attackerId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Set<BattleshipAttack>()
            .Where(a => a.GameId == gameId && a.AttackerId == attackerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<BattleshipGameStats> GetStatsForUserAsync(
        string userId,
        CancellationToken cancellationToken = default
    )
    {
        var finishedGames = await _dbSet
            .Where(g =>
                (g.CreatorId == userId || g.OpponentId == userId)
                && (
                    g.Status == GameStatus.Finished_Winner
                    || g.Status == GameStatus.Finished_Abandoned
                )
            )
            .ToListAsync(cancellationToken);

        var totalGames = finishedGames.Count;
        var wonGames = finishedGames.Count(g => g.WinnerId == userId);
        var lostGames = totalGames - wonGames;

        return new BattleshipGameStats(totalGames, wonGames, lostGames);
    }

    private static IQueryable<BattleshipGame> ApplyOptionsToQuery(
        IQueryable<BattleshipGame> query,
        QueryOptions<BattleshipGame>? options
    )
    {
        if (options is null)
            return query.AsNoTracking();

        if (!options.IsTracking)
            query = query.AsNoTracking();

        foreach (var include in options.Includes)
            query = query.Include(include);

        if (options.Filter is not null)
            query = query.Where(options.Filter);

        if (options.OrderBy is not null)
            query = options.OrderBy(query);

        if (options.Skip.HasValue)
            query = query.Skip(options.Skip.Value);

        if (options.Take.HasValue)
            query = query.Take(options.Take.Value);

        return query;
    }
}
