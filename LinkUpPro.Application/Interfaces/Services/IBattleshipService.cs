using LinkUpPro.Application.DTOs.Battleship.Requests;
using LinkUpPro.Application.DTOs.Battleship.Responses;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.Interfaces.Services;

public interface IBattleshipService
{
    Task<Result<GameResponseDto>> CreateGameAsync(string creatorId, CreateGameRequest request);

    Task<PagedResult<GameListItemDto>> GetActiveGamesAsync(string userId, int page = 1, int pageSize = 20);

    Task<PagedResult<GameListItemDto>> GetGameHistoryAsync(string userId, int page = 1, int pageSize = 20);

    Task<Result<GameStatsDto>> GetStatsAsync(string userId);

    Task<Result> PlaceShipAsync(string userId, long gameId, PlaceShipRequest request);

    Task<Result<AttackResultDto>> AttackAsync(string userId, long gameId, AttackRequest request);

    Task<Result> SurrenderAsync(string userId, SurrenderRequest request);
}
