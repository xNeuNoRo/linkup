using LinkUpPro.Application.DTOs.Battleship.Requests;
using LinkUpPro.Application.DTOs.Battleship.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Entities.Battleship;
using LinkUpPro.Domain.Entities.Social;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.Exceptions;
using LinkUpPro.Domain.Interfaces.Persistence;
using LinkUpPro.Domain.Interfaces.Repositories;
using LinkUpPro.Domain.ValueObjects;
using Mapster;

namespace LinkUpPro.Application.Services;

public sealed class BattleshipService : IBattleshipService
{
    private readonly IBattleshipRepository _battleshipRepository;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IProfileService _profileService;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BattleshipService(
        IBattleshipRepository battleshipRepository,
        IFriendshipRepository friendshipRepository,
        IProfileService profileService,
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork
    )
    {
        _battleshipRepository = battleshipRepository;
        _friendshipRepository = friendshipRepository;
        _profileService = profileService;
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<GameResponseDto>> CreateGameAsync(
        string creatorId,
        CreateGameRequest request
    )
    {
        var gameResult = BattleshipGame.Create(creatorId, request.OpponentId);
        if (gameResult.IsFailure)
            return Result<GameResponseDto>.Failure(gameResult.Errors);

        var areFriends = await _friendshipRepository.AreFriendsAsync(creatorId, request.OpponentId);
        if (!areFriends)
            return Result<GameResponseDto>.Failure(
                new DomainError(
                    "Battleship.NotFriends",
                    "Solo puede iniciar una partida con un amigo."
                )
            );

        var hasActiveGame = await _battleshipRepository.HasActiveGameBetweenAsync(
            creatorId,
            request.OpponentId
        );
        if (hasActiveGame)
            return Result<GameResponseDto>.Failure(
                new DomainError(
                    "Battleship.ActiveGameExists",
                    "Ya existe una partida activa entre estos usuarios."
                )
            );

        await _battleshipRepository.AddAsync(gameResult.Value);
        await _unitOfWork.SaveChangesAsync();

        // Notify opponent about the invitation
        var creator = await _profileService.GetByIdAsync(creatorId);
        var creatorName = creator is null
            ? "Alguien"
            : $"{creator.FirstName} {creator.LastName}".Trim();
        var inviteNotif = Notification.CreateBattleshipGameInvited(
            recipientId: request.OpponentId,
            actorId: creatorId,
            gameId: gameResult.Value.Id,
            actorUserName: creatorName
        );
        if (inviteNotif.IsSuccess)
            await _notificationRepository.AddAsync(inviteNotif.Value);
        await _unitOfWork.SaveChangesAsync();

        return Result<GameResponseDto>.Success(gameResult.Value.Adapt<GameResponseDto>());
    }

    public async Task<Result> PlaceShipAsync(string userId, long gameId, PlaceShipRequest request)
    {
        var game = await _battleshipRepository.GetByIdAsync(gameId);
        if (game is null)
            return Result.Failure(
                new DomainError("Battleship.NotFound", "La partida no fue encontrada.")
            );

        var abandonment = game.CheckAbandonment(DateTimeOffset.UtcNow);
        if (abandonment.IsFailure)
            return abandonment;

        ShipPlacement placement;
        try
        {
            var start = Coordinates.Create(request.StartX, request.StartY);
            var direction = (ShipDirection)request.Direction;
            var size = (ShipSize)request.ShipSize;
            placement = ShipPlacement.Create(start, direction, size);
        }
        catch (DomainException ex)
        {
            return Result.Failure(new DomainError(ex.Code, ex.Message));
        }

        var existingShips = await _battleshipRepository.GetShipsByGameAndPlayerAsync(
            gameId,
            userId
        );
        var existingPlacements = existingShips
            .Select(s =>
                ShipPlacement.Create(Coordinates.Create(s.StartX, s.StartY), s.Direction, s.Size)
            )
            .ToList();
        if (placement.OverlapsWith(existingPlacements))
            return Result.Failure(
                new DomainError(
                    "Battleship.OverlappingShips",
                    "Debe cambiar la celda seleccionada o la direccion, ya que con la combinacion actual el barco quedaria posicionado encima de otro barco."
                )
            );

        var shipResult = BattleshipShip.Place(gameId, userId, placement);
        if (shipResult.IsFailure)
            return Result.Failure(shipResult.Errors);

        var updatedShips = existingShips.Append(shipResult.Value).ToList();
        if (updatedShips.Count == DomainConstants.BattleshipFleetShipCount)
        {
            var completion = game.CompletePlayerPlacement(userId, updatedShips);
            if (completion.IsFailure)
                return Result.Failure(completion.Errors);
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _battleshipRepository.AddShipAsync(shipResult.Value);

            if (updatedShips.Count == DomainConstants.BattleshipFleetShipCount)
                _battleshipRepository.Update(game);

            await _unitOfWork.CommitAsync();

            // Notify opponent when game starts (both players placed all ships)
            if (game.Status == GameStatus.InProgress)
            {
                var player = await _profileService.GetByIdAsync(userId);
                var playerName = player is null
                    ? "Alguien"
                    : $"{player.FirstName} {player.LastName}".Trim();
                var opponentId = game.GetOpponentId(userId);
                if (opponentId.IsSuccess)
                {
                    var startNotif = Notification.CreateBattleshipGameStarted(
                        recipientId: opponentId.Value,
                        actorId: userId,
                        gameId: gameId,
                        actorUserName: playerName
                    );
                    if (startNotif.IsSuccess)
                        await _notificationRepository.AddAsync(startNotif.Value);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            return Result.Success();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<Result<AttackResultDto>> AttackAsync(
        string userId,
        long gameId,
        AttackRequest request
    )
    {
        var game = await _battleshipRepository.GetByIdAsync(gameId);
        if (game is null)
            return Result<AttackResultDto>.Failure(
                new DomainError("Battleship.NotFound", "La partida no fue encontrada.")
            );

        var abandonment = game.CheckAbandonment(DateTimeOffset.UtcNow);
        if (abandonment.IsFailure)
            return Result<AttackResultDto>.Failure(abandonment.Errors);

        Coordinates target;
        try
        {
            target = Coordinates.Create(request.X, request.Y);
        }
        catch (DomainException ex)
        {
            return Result<AttackResultDto>.Failure(new DomainError(ex.Code, ex.Message));
        }

        var opponentId = game.GetOpponentId(userId);
        if (opponentId.IsFailure)
            return Result<AttackResultDto>.Failure(opponentId.Errors);

        // Cargar solo los barcos del oponente (no todos los barcos de la partida)
        var opponentShips = await _battleshipRepository.GetShipsByGameAndPlayerAsync(
            gameId,
            opponentId.Value
        );

        // Cargar solo los ataques de ESTE jugador (no todos los ataques de la partida)
        var playerAttacks = await _battleshipRepository.GetAttacksByGameAndAttackerAsync(
            gameId,
            userId
        );

        var attackResult = game.ExecuteAttack(userId, target, opponentShips, playerAttacks);
        if (attackResult.IsFailure)
            return Result<AttackResultDto>.Failure(attackResult.Errors);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _battleshipRepository.AddAttackAsync(attackResult.Value);
            _battleshipRepository.Update(game);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        var isSunk =
            attackResult.Value.TargetShipId.HasValue
            && opponentShips.Where(s => s.Id == attackResult.Value.TargetShipId.Value).Any(s => s.IsSunk);

        if (isSunk)
        {
            var targetShipId = attackResult.Value.TargetShipId!.Value;
            var attacker = await _profileService.GetByIdAsync(userId);
            var attackerName = attacker is null
                ? "Alguien"
                : $"{attacker.FirstName} {attacker.LastName}".Trim();
            var sunkShip = opponentShips.First(s => s.Id == targetShipId);
            var shipSize = (int)sunkShip.Size;

            // Notify defender that their ship was sunk
            var sunkNotif = Notification.CreateBattleshipShipSunk(
                recipientId: opponentId.Value,
                actorId: userId,
                gameId: gameId,
                shipSize: shipSize,
                actorUserName: attackerName
            );
            if (sunkNotif.IsSuccess)
                await _notificationRepository.AddAsync(sunkNotif.Value);

            // Notify attacker that they sunk a ship
            // Use the defender as actor (ship owner) to avoid self-notification rule
            var opponentProfile = await _profileService.GetByIdAsync(opponentId.Value);
            var opponentName = opponentProfile is null
                ? "Alguien"
                : $"{opponentProfile.FirstName} {opponentProfile.LastName}".Trim();
            var sunkByNotif = Notification.CreateBattleshipShipSunkByOpponent(
                recipientId: userId,
                actorId: opponentId.Value,
                gameId: gameId,
                shipSize: shipSize,
                actorUserName: opponentName
            );
            if (sunkByNotif.IsSuccess)
                await _notificationRepository.AddAsync(sunkByNotif.Value);

            await _unitOfWork.SaveChangesAsync();
        }

        return Result<AttackResultDto>.Success(
            new AttackResultDto(
                IsHit: attackResult.Value.IsHit,
                IsSunk: isSunk,
                IsGameOver: game.IsFinished(),
                WinnerId: game.WinnerId,
                TurnChanged: true
            )
        );
    }

    public async Task<Result> SurrenderAsync(string userId, SurrenderRequest request)
    {
        var game = await _battleshipRepository.GetByIdAsync(request.GameId);
        if (game is null)
            return Result.Failure(
                new DomainError("Battleship.NotFound", "La partida no fue encontrada.")
            );

        var surrender = game.Surrender(userId);
        if (surrender.IsFailure)
            return surrender;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            _battleshipRepository.Update(game);
            await _unitOfWork.CommitAsync();
            return Result.Success();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<PagedResult<GameListItemDto>> GetActiveGamesAsync(
        string userId,
        int page = 1,
        int pageSize = 20
    )
    {
        var options = new QueryOptions<BattleshipGame>
        {
            Skip = (page - 1) * pageSize,
            Take = pageSize,
            OrderBy = q => q.OrderByDescending(g => g.StartedAt),
            IsTracking = false,
        };

        var games = await _battleshipRepository.GetActiveGamesForUserAsync(userId, options);
        var items = await MapToListItemDtosAsync(games, userId);
        var total = await _battleshipRepository.CountAsync(g =>
            (g.CreatorId == userId || g.OpponentId == userId)
            && (
                g.Status == GameStatus.Configuring_P1
                || g.Status == GameStatus.Configuring_P2
                || g.Status == GameStatus.InProgress
            )
        );

        return new PagedResult<GameListItemDto>(items, total, page, pageSize);
    }

    public async Task<PagedResult<GameListItemDto>> GetGameHistoryAsync(
        string userId,
        int page = 1,
        int pageSize = 20
    )
    {
        var options = new QueryOptions<BattleshipGame>
        {
            Skip = (page - 1) * pageSize,
            Take = pageSize,
            OrderBy = q => q.OrderByDescending(g => g.FinishedAt ?? g.StartedAt),
            IsTracking = false,
        };

        var games = await _battleshipRepository.GetFinishedGamesForUserAsync(userId, options);
        var items = await MapToListItemDtosAsync(games, userId);
        var total = await _battleshipRepository.CountAsync(g =>
            (g.CreatorId == userId || g.OpponentId == userId)
            && (g.Status == GameStatus.Finished_Winner || g.Status == GameStatus.Finished_Abandoned)
        );

        return new PagedResult<GameListItemDto>(items, total, page, pageSize);
    }

    public async Task<Result<GameStatsDto>> GetStatsAsync(string userId)
    {
        var stats = await _battleshipRepository.GetStatsForUserAsync(userId);
        return Result<GameStatsDto>.Success(
            new GameStatsDto(stats.TotalGames, stats.WonGames, stats.LostGames)
        );
    }

    private async Task<List<GameListItemDto>> MapToListItemDtosAsync(
        IReadOnlyCollection<BattleshipGame> games,
        string currentUserId
    )
    {
        if (games.Count == 0)
            return new List<GameListItemDto>();

        var items = new List<GameListItemDto>(games.Count);

        var opponentIds = games
            .Select(g => g.CreatorId == currentUserId ? g.OpponentId : g.CreatorId)
            .Distinct()
            .ToList();

        var userDict = await _profileService.GetByIdsAsync(opponentIds);

        foreach (var game in games)
        {
            var opponentId = game.CreatorId == currentUserId ? game.OpponentId : game.CreatorId;
            userDict.TryGetValue(opponentId, out var opponent);
            var opponentName = opponent is null
                ? "Desconocido"
                : $"{opponent.FirstName} {opponent.LastName}".Trim();

            var duration = game.FinishedAt.HasValue
                ? game.FinishedAt.Value - game.StartedAt
                : (TimeSpan?)null;

            items.Add(
                new GameListItemDto(
                    game.Id,
                    opponentId,
                    opponentName,
                    game.Status,
                    game.StartedAt,
                    game.FinishedAt,
                    game.WinnerId,
                    game.CurrentTurnUserId,
                    duration
                )
            );
        }

        return items;
    }

    public async Task<Result<GameDetailDto>> GetGameDetailAsync(string userId, long gameId)
    {
        var game = await _battleshipRepository.GetByIdAsync(gameId);
        if (game is null || !game.IsPlayerInGame(userId))
            return Result<GameDetailDto>.Failure(
                new DomainError("Battleship.NotFound", "La partida no fue encontrada.")
            );

        var previousStatus = game.Status;
        game.CheckAbandonment(DateTimeOffset.UtcNow);

        // Si el estado cambió por abandono, persistir el cambio
        if (game.Status != previousStatus)
        {
            _battleshipRepository.Update(game);
            await _unitOfWork.SaveChangesAsync();
        }

        var opponentId = game.GetOpponentId(userId).Value;
        var opponent = await _profileService.GetByIdAsync(opponentId);
        var opponentName = opponent is null
            ? "Desconocido"
            : $"{opponent.FirstName} {opponent.LastName}".Trim();

        return Result<GameDetailDto>.Success(
            new GameDetailDto(
                game.Id,
                opponentId,
                opponentName,
                game.Status,
                game.StartedAt,
                game.CurrentTurnUserId,
                game.CurrentTurnUserId == userId,
                game.IsFinished(),
                game.WinnerId,
                game.GetTurnElapsedTime(DateTimeOffset.UtcNow)
            )
        );
    }

    public async Task<Result<AttackBoardDto>> GetMyAttackBoardAsync(string userId, long gameId)
    {
        var game = await _battleshipRepository.GetByIdAsync(gameId);
        if (game is not null && game.IsPlayerInGame(userId))
        {
            var previousStatus = game.Status;
            game.CheckAbandonment(DateTimeOffset.UtcNow);
            if (game.Status != previousStatus)
            {
                _battleshipRepository.Update(game);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        return await GetAttackBoardAsync(userId, gameId, userId);
    }

    public async Task<Result<AttackBoardDto>> GetOpponentAttackBoardAsync(
        string userId,
        long gameId
    )
    {
        var game = await _battleshipRepository.GetByIdAsync(gameId);
        if (game is null || !game.IsPlayerInGame(userId))
            return Result<AttackBoardDto>.Failure(
                new DomainError("Battleship.NotFound", "La partida no fue encontrada.")
            );

        var opponentId = game.GetOpponentId(userId).Value;
        return await GetAttackBoardAsync(userId, gameId, opponentId);
    }

    public async Task<Result<PlacementBoardDto>> GetMyPlacementBoardAsync(
        string userId,
        long gameId
    )
    {
        return await GetPlacementBoardAsync(userId, gameId, userId);
    }

    public async Task<Result<PlacementBoardDto>> GetOpponentPlacementBoardAsync(
        string userId,
        long gameId
    )
    {
        var game = await _battleshipRepository.GetByIdAsync(gameId);
        if (game is null || !game.IsPlayerInGame(userId))
            return Result<PlacementBoardDto>.Failure(
                new DomainError("Battleship.NotFound", "La partida no fue encontrada.")
            );

        var opponentId = game.GetOpponentId(userId).Value;
        return await GetPlacementBoardAsync(userId, gameId, opponentId);
    }

    public async Task<Result<GameResultDto>> GetGameResultAsync(string userId, long gameId)
    {
        var game = await _battleshipRepository.GetGameWithShipsAndAttacksAsync(gameId);
        if (game is null || !game.IsPlayerInGame(userId))
            return Result<GameResultDto>.Failure(
                new DomainError("Battleship.NotFound", "La partida no fue encontrada.")
            );

        if (!game.IsFinished())
            return Result<GameResultDto>.Failure(
                new DomainError("Battleship.NotFinished", "La partida aun no ha finalizado.")
            );

        var opponentId = game.GetOpponentId(userId).Value;
        var opponent = await _profileService.GetByIdAsync(opponentId);
        var opponentName = opponent is null
            ? "Desconocido"
            : $"{opponent.FirstName} {opponent.LastName}".Trim();

        var duration = game.FinishedAt.HasValue
            ? (game.FinishedAt.Value - game.StartedAt).TotalHours
            : 0.0;

        var result = game.WinnerId == userId ? "Ganada" : "Perdida";
        var winner =
            game.WinnerId == userId ? "Yo"
            : game.WinnerId == opponentId ? opponentName
            : "Desconocido";

        var ships = await _battleshipRepository.GetShipsByGameAsync(gameId);
        var allAttacks = await _battleshipRepository.GetAttacksByGameAsync(gameId);

        var myAttackBoard = BuildAttackBoardFromData(game, allAttacks, userId);
        var opponentAttackBoard = BuildAttackBoardFromData(game, allAttacks, opponentId);
        var myPlacementBoard = BuildPlacementBoardFromData(ships, gameId, userId);

        return Result<GameResultDto>.Success(
            new GameResultDto(
                game.Id,
                opponentId,
                opponentName,
                game.StartedAt,
                game.FinishedAt,
                duration,
                result,
                winner,
                myAttackBoard,
                opponentAttackBoard,
                myPlacementBoard
            )
        );
    }

    private static AttackBoardDto BuildAttackBoardFromData(
        BattleshipGame game,
        IReadOnlyCollection<BattleshipAttack> allAttacks,
        string attackerId
    )
    {
        var playerAttacks = allAttacks.Where(a => a.AttackerId == attackerId).ToArray();
        var grid = new BoardCellState[DomainConstants.BoardSize, DomainConstants.BoardSize];

        foreach (var attack in playerAttacks)
        {
            grid[attack.TargetX, attack.TargetY] = attack.IsHit
                ? BoardCellState.Hit
                : BoardCellState.Miss;
        }

        return new AttackBoardDto(
            game.Id,
            attackerId,
            grid,
            game.CurrentTurnUserId,
            game.CurrentTurnUserId == attackerId,
            game.IsFinished(),
            game.WinnerId
        );
    }

    private static PlacementBoardDto BuildPlacementBoardFromData(
        IReadOnlyCollection<BattleshipShip> allShips,
        long gameId,
        string playerId
    )
    {
        var ships = allShips.Where(s => s.PlayerId == playerId).ToArray();
        var shipDtos = ships.Select(s =>
        {
            var cells = s.GetOccupiedCells()
                .Select(c => new[] { (int)c.X, (int)c.Y })
                .ToArray();
            return new ShipPlacementDto(s.Id, s.Size, s.StartX, s.StartY, s.Direction, s.IsSunk, cells);
        }).ToArray();

        return new PlacementBoardDto(gameId, playerId, shipDtos);
    }

    private async Task<Result<AttackBoardDto>> GetAttackBoardAsync(
        string requesterId,
        long gameId,
        string attackerId
    )
    {
        var game = await _battleshipRepository.GetByIdAsync(gameId);
        if (game is null || !game.IsPlayerInGame(requesterId))
            return Result<AttackBoardDto>.Failure(
                new DomainError("Battleship.NotFound", "La partida no fue encontrada.")
            );

        var attacks = await _battleshipRepository.GetAttacksByGameAndAttackerAsync(
            gameId,
            attackerId
        );

        var grid = new BoardCellState[DomainConstants.BoardSize, DomainConstants.BoardSize];

        foreach (var attack in attacks)
        {
            grid[attack.TargetX, attack.TargetY] = attack.IsHit
                ? BoardCellState.Hit
                : BoardCellState.Miss;
        }

        return Result<AttackBoardDto>.Success(
            new AttackBoardDto(
                gameId,
                attackerId,
                grid,
                game.CurrentTurnUserId,
                game.CurrentTurnUserId == requesterId,
                game.IsFinished(),
                game.WinnerId
            )
        );
    }

    private async Task<Result<PlacementBoardDto>> GetPlacementBoardAsync(
        string requesterId,
        long gameId,
        string playerId
    )
    {
        var game = await _battleshipRepository.GetByIdAsync(gameId);
        if (game is null || !game.IsPlayerInGame(requesterId))
            return Result<PlacementBoardDto>.Failure(
                new DomainError("Battleship.NotFound", "La partida no fue encontrada.")
            );

        var ships = await _battleshipRepository.GetShipsByGameAndPlayerAsync(gameId, playerId);

        var shipDtos = ships
            .Select(s =>
            {
                var cells = s.GetOccupiedCells()
                    .Select(c => new[] { (int)c.X, (int)c.Y })
                    .ToArray();
                return new ShipPlacementDto(
                    s.Id,
                    s.Size,
                    s.StartX,
                    s.StartY,
                    s.Direction,
                    s.IsSunk,
                    cells
                );
            })
            .ToArray();

        return Result<PlacementBoardDto>.Success(new PlacementBoardDto(gameId, playerId, shipDtos));
    }
}
