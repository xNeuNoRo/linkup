using LinkUpPro.Application.DTOs.Battleship.Requests;
using LinkUpPro.Application.DTOs.Battleship.Responses;
using LinkUpPro.Application.Interfaces.Services;
using LinkUpPro.Application.ViewModels.BattleshipViewModels;
using LinkUpPro.Application.ViewModels.FriendshipViewModels;
using LinkUpPro.Application.ViewModels.Shared;
using LinkUpPro.Domain.Enums;
using LinkUpPro.WebApp.Extensions;
using LinkUpPro.WebApp.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.WebApp.Controllers;

/// <summary>
/// Controlador del módulo de Battleship: dashboard, creación, colocación de barcos, ataques, historial.
/// </summary>
[SessionAuthorize]
public class BattleshipController : BaseController
{
    private readonly IBattleshipService _battleshipService;
    private readonly IFriendshipService _friendshipService;
    private readonly IFriendRequestService _friendRequestService;
    private readonly INotificationService _notificationService;
    private readonly IProfileService _profileService;
    private readonly ILogger<BattleshipController> _logger;

    public BattleshipController(
        IBattleshipService battleshipService,
        IFriendshipService friendshipService,
        IFriendRequestService friendRequestService,
        INotificationService notificationService,
        IProfileService profileService,
        ICurrentUserService currentUserService,
        ILogger<BattleshipController> logger
    )
        : base(currentUserService)
    {
        _battleshipService = battleshipService;
        _friendshipService = friendshipService;
        _friendRequestService = friendRequestService;
        _notificationService = notificationService;
        _profileService = profileService;
        _logger = logger;
    }

    // ====================== INDEX (Dashboard) ======================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _currentUserService.UserId!;

        var activePaged = await _battleshipService.GetActiveGamesAsync(userId);
        var historyPaged = await _battleshipService.GetGameHistoryAsync(userId);
        var statsResult = await _battleshipService.GetStatsAsync(userId);

        var activeList = activePaged.Items.ToList();
        var historyList = historyPaged.Items.ToList();
        var activeVm = new List<ActiveGameListItemViewModel>(activeList.Count);
        foreach (var d in activeList) activeVm.Add(MapToActiveGameListItem(d, userId));
        var historyVm = new List<GameHistoryItemViewModel>(historyList.Count);
        foreach (var d in historyList) historyVm.Add(MapToGameListItemToHistory(d, userId));

        var vm = new GameDetailViewModel
        {
            ActiveGames = activeVm,
            History = historyVm,
            Stats = statsResult.IsSuccess ? MapToGameStats(statsResult.Value!) : new GameStatsViewModel()
        };

        await this.PopulateMenuCountersAsync(_currentUserService, _friendRequestService, _notificationService);
        return View(vm);
    }

    // ====================== CREATE GAME (GET) ======================

    [HttpGet]
    public async Task<IActionResult> CreateGame(string? search)
    {
        var userId = _currentUserService.UserId!;
        var pagedResult = await _battleshipService.GetActiveGamesAsync(userId, 1, 50);

        var activeOpponentIds = pagedResult.Items.Select(g => g.OpponentId).ToHashSet();

        var allFriendsPaged = await GetAllFriendsAsync(userId);

        // Filtrar los que NO tienen partida activa
        var availableOpponents = allFriendsPaged
            .Where(f => !activeOpponentIds.Contains(f.FriendId))
            .Where(f => string.IsNullOrEmpty(search) || f.FriendUserName.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var vm = new CreateGameViewModel
        {
            SearchText = search,
            AvailableOpponents = availableOpponents.Select(MapToAvailableOpponent).ToList()
        };

        var dashboardVm = new GameDetailViewModel();
        await this.PopulateMenuCountersAsync(_currentUserService, _friendRequestService, _notificationService);
        ViewBag.CurrentUserId = userId;

        return View(vm);
    }

    // ====================== SEARCH OPPONENTS (AJAX Partial) ======================

    [HttpGet]
    public async Task<IActionResult> SearchOpponents(string? search)
    {
        var userId = _currentUserService.UserId!;
        var pagedResult = await _battleshipService.GetActiveGamesAsync(userId, 1, 50);
        var activeOpponentIds = pagedResult.Items.Select(g => g.OpponentId).ToHashSet();
        var allFriendsPaged = await GetAllFriendsAsync(userId);

        var availableOpponents = allFriendsPaged
            .Where(f => !activeOpponentIds.Contains(f.FriendId))
            .Where(f => string.IsNullOrEmpty(search) || f.FriendUserName.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var vm = new CreateGameViewModel
        {
            SearchText = search,
            AvailableOpponents = availableOpponents.Select(MapToAvailableOpponent).ToList()
        };

        return PartialView("_OpponentList", vm);
    }

    // ====================== CREATE GAME (POST) ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateGame(CreateGameViewModel model)
    {
        if (string.IsNullOrEmpty(model.SelectedOpponentId))
        {
            ModelState.AddModelError(string.Empty, "Debe seleccionar un amigo para iniciar la partida.");
            return await CreateGame(model.SearchText);
        }

        try
        {
            var result = await _battleshipService.CreateGameAsync(
                _currentUserService.UserId!,
                new CreateGameRequest(model.SelectedOpponentId)
            );

            if (!result.IsSuccess)
            {
                ShowError(result.Error?.Message ?? "No se pudo crear la partida.");
                return RedirectToAction(nameof(CreateGame));
            }

            ShowAlert("Partida creada. Ahora debe colocar sus barcos.");
            return RedirectToAction(nameof(Placement), new { gameId = result.Value!.Id });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error creando partida");
            ShowError("No se pudo crear la partida.");
            return RedirectToAction(nameof(CreateGame));
        }
    }

    // ====================== PLACEMENT (Unified: select ships + board + direction in one view) ======================

    [HttpGet]
    public async Task<IActionResult> Placement(long gameId)
    {
        var userId = _currentUserService.UserId!;
        var detailResult = await _battleshipService.GetGameDetailAsync(userId, gameId);

        if (!detailResult.IsSuccess)
        {
            ShowError(detailResult.Error?.Message ?? "No se pudo cargar la partida.");
            return RedirectToAction(nameof(Index));
        }

        var detail = detailResult.Value!;

        if (detail.Status == GameStatus.InProgress)
            return RedirectToAction(nameof(AttackBoard), new { gameId });

        if (detail.Status == GameStatus.Finished_Winner || detail.Status == GameStatus.Finished_Abandoned)
            return RedirectToAction(nameof(Result), new { gameId });

        var placementResult = await _battleshipService.GetMyPlacementBoardAsync(userId, gameId);
        var placement = placementResult.Value!;

        var placedShips = placement.Ships.Select(MapToPlacedShip).ToList();
        var board = BuildBoardFromPlacedShips(placedShips);

        var vm = new ShipPlacementViewModel
        {
            GameId = gameId,
            ShipsToPlace = BuildShipsToPlace(placement.Ships),
            PlacedShips = placedShips,
            Board = board,
            OpponentHasFinishedPlacement = false
        };

        await this.PopulateMenuCountersAsync(_currentUserService, _friendRequestService, _notificationService);
        ViewBag.CurrentUserId = userId;

        return View("Placement/Index", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceShip(ShipPlacementViewModel model)
    {
        var userId = _currentUserService.UserId!;

        if (!model.SelectedShipSize.HasValue)
        {
            ShowError("Debe seleccionar un barco para posicionar.");
            return RedirectToAction(nameof(Placement), new { gameId = model.GameId });
        }

        try
        {
            if (!int.TryParse(Request.Form["Direction"], out var direction) ||
                !int.TryParse(Request.Form["StartX"], out var startX) ||
                !int.TryParse(Request.Form["StartY"], out var startY))
            {
                ShowError("Debe seleccionar una celda y una dirección para posicionar el barco.");
                return RedirectToAction(nameof(Placement), new { gameId = model.GameId });
            }

            // View sends direction 0-3 (Up=0, Down=1, Left=2, Right=3);
            // ShipDirection enum is 1-4 (Up=1, Down=2, Left=3, Right=4).
            direction += 1;

            var request = new PlaceShipRequest(
                model.SelectedShipSize.Value,
                startX,
                startY,
                direction
            );

            var result = await _battleshipService.PlaceShipAsync(userId, model.GameId, request);

            if (!result.IsSuccess)
            {
                ShowError(result.Error?.Message ?? "No se pudo colocar el barco.");
                return RedirectToAction(nameof(Placement), new { gameId = model.GameId });
            }

            ShowAlert("Barco posicionado correctamente.");

            return RedirectToAction(nameof(Placement), new { gameId = model.GameId });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error colocando barco");
            ShowError("No se pudo colocar el barco.");
            return RedirectToAction(nameof(Placement), new { gameId = model.GameId });
        }
    }

    // ====================== ATTACK BOARD ======================

    [HttpGet]
    public async Task<IActionResult> AttackBoard(long gameId)
    {
        var userId = _currentUserService.UserId!;
        var boardResult = await _battleshipService.GetMyAttackBoardAsync(userId, gameId);

        if (!boardResult.IsSuccess)
        {
            ShowError(boardResult.Error?.Message ?? "No se pudo cargar el tablero de ataque.");
            return RedirectToAction(nameof(Index));
        }

        var board = boardResult.Value!;
        var detailResult = await _battleshipService.GetGameDetailAsync(userId, gameId);
        var detail = detailResult.IsSuccess ? detailResult.Value : null;

        var opponentId = detail?.OpponentId ?? "";
        var opponent = await _profileService.GetByIdAsync(opponentId);

        // Load placement board for inline toggle
        var placementResult = await _battleshipService.GetMyPlacementBoardAsync(userId, gameId);
        CellViewModel[,]? placementBoard = null;
        if (placementResult.IsSuccess)
        {
            var opponentAttacksResult = await _battleshipService.GetOpponentAttackBoardAsync(userId, gameId);
            placementBoard = BuildFullPlacementBoard(placementResult.Value!, opponentAttacksResult.IsSuccess ? opponentAttacksResult.Value : null);
        }

        var vm = new AttackBoardViewModel
        {
            GameId = gameId,
            OpponentId = opponentId,
            OpponentName = opponent != null ? $"{opponent.FirstName} {opponent.LastName}".Trim() : "Oponente",
            IsMyTurn = board.IsMyTurn,
            IsGameOver = board.IsGameOver,
            WinnerId = board.WinnerId,
            CurrentTurnUserId = board.CurrentTurnUserId,
            CurrentUserId = userId,
            Board = ConvertToCellArray(board.Grid),
            MyPlacementBoard = placementBoard ?? new CellViewModel[12, 12],
            TurnMessage = board.IsGameOver
                ? "Partida finalizada"
                : board.IsMyTurn
                    ? "Es tu turno de atacar"
                    : $"Es turno de {(opponent != null ? opponent.UserName : "oponente")} de atacar"
        };

        await this.PopulateMenuCountersAsync(_currentUserService, _friendRequestService, _notificationService);
        return View("Attack/AttackBoard", vm);
    }

    // ====================== EXECUTE ATTACK ======================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExecuteAttack(long gameId, int targetX, int targetY)
    {
        var isAjax = Request.Headers.XRequestedWith == "XMLHttpRequest";

        try
        {
            var result = await _battleshipService.AttackAsync(
                _currentUserService.UserId!,
                gameId,
                new AttackRequest(targetX, targetY)
            );

            if (isAjax)
            {
                if (!result.IsSuccess)
                    return Json(new { success = false, message = result.Error?.Message ?? "No se pudo realizar el ataque." });

                return Json(new
                {
                    success = true,
                    isHit = result.Value!.IsHit,
                    isSunk = result.Value.IsSunk,
                    isGameOver = result.Value.IsGameOver,
                    winnerId = result.Value.WinnerId,
                    message = result.Value.IsHit ? "¡Acierto!" : "Agua"
                });
            }

            if (!result.IsSuccess)
                ShowError(result.Error?.Message ?? "No se pudo realizar el ataque.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error ejecutando ataque");
            if (isAjax)
                return Json(new { success = false, message = "Error al procesar el ataque." });
            ShowError("No se pudo realizar el ataque.");
        }

        return RedirectToAction(nameof(AttackBoard), new { gameId });
    }

    // ====================== REFRESH TURN ======================

    [HttpGet]
    public async Task<IActionResult> RefreshTurn(long gameId)
    {
        return await AttackBoard(gameId);
    }

    // ====================== GAME STATE (JSON for AJAX polling) ======================

    [HttpGet]
    public async Task<IActionResult> GetGameState(long gameId)
    {
        var userId = _currentUserService.UserId!;
        var boardResult = await _battleshipService.GetMyAttackBoardAsync(userId, gameId);
        if (!boardResult.IsSuccess)
            return Json(new { error = "No se pudo cargar el estado." });

        var board = boardResult.Value!;
        var detailResult = await _battleshipService.GetGameDetailAsync(userId, gameId);
        var detail = detailResult.IsSuccess ? detailResult.Value : null;
        var opponentId = detail?.OpponentId ?? "";
        var opponent = await _profileService.GetByIdAsync(opponentId);

        return Json(new
        {
            isMyTurn = board.IsMyTurn,
            isGameOver = board.IsGameOver,
            winnerId = board.WinnerId,
            currentTurnUserId = board.CurrentTurnUserId,
            turnMessage = board.IsGameOver
                ? "Partida finalizada"
                : board.IsMyTurn
                    ? "Es tu turno de atacar"
                    : $"Es turno de {(opponent != null ? opponent.UserName : "oponente")} de atacar"
        });
    }

    // ====================== MY BOARD ======================

    [HttpGet]
    public async Task<IActionResult> MyBoard(long gameId)
    {
        var userId = _currentUserService.UserId!;
        var placementResult = await _battleshipService.GetMyPlacementBoardAsync(userId, gameId);
        var attackResult = await _battleshipService.GetMyAttackBoardAsync(userId, gameId);

        if (!placementResult.IsSuccess)
        {
            ShowError("No se pudo cargar el tablero.");
            return RedirectToAction(nameof(AttackBoard), new { gameId });
        }

        var placement = placementResult.Value!;
        var attack = attackResult.IsSuccess ? attackResult.Value : null;

        // Construir matriz de celdas con info de barcos
        var board = new CellViewModel[12, 12];
        for (var r = 0; r < 12; r++)
        for (var c = 0; c < 12; c++)
            board[r, c] = new CellViewModel { X = c, Y = r, State = BoardCellState.Empty };

        foreach (var ship in placement.Ships)
        {
            foreach (var cell in ship.OccupiedCells)
            {
                var x = cell[0];
                var y = cell[1];
                if (x >= 0 && x < 12 && y >= 0 && y < 12)
                    board[y, x] = new CellViewModel
                    {
                        X = x,
                        Y = y,
                        State = ship.IsSunk ? BoardCellState.Sunk : BoardCellState.Ship,
                        ShipId = ship.Id,
                        ShipSize = (int)ship.Size
                    };
            }
        }

        // Construir tablero de ataques recibidos del oponente
        CellViewModel[,]? receivedAttacks = null;
        var opponentAttacksResult = await _battleshipService.GetOpponentAttackBoardAsync(userId, gameId);
        if (opponentAttacksResult.IsSuccess)
        {
            var opponentAttacks = opponentAttacksResult.Value!;
            receivedAttacks = new CellViewModel[12, 12];
            for (var r = 0; r < 12; r++)
            for (var c = 0; c < 12; c++)
                receivedAttacks[r, c] = new CellViewModel { X = c, Y = r, State = opponentAttacks.Grid[c, r] };
        }

        var vm = new MyPlacementBoardViewModel
        {
            GameId = gameId,
            Board = board,
            ReceivedAttacksBoard = receivedAttacks
        };

        return View("Attack/MyBoard", vm);
    }

    // ====================== SURRENDER ======================

    [HttpGet]
    public IActionResult Surrender(long gameId)
    {
        return View("Attack/SurrenderConfirm", new SurrenderConfirmViewModel { GameId = gameId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SurrenderConfirm(SurrenderConfirmViewModel model)
    {
        try
        {
            var result = await _battleshipService.SurrenderAsync(
                _currentUserService.UserId!,
                new SurrenderRequest(model.GameId)
            );

            if (!result.IsSuccess)
            {
                ShowError(result.Error?.Message ?? "No se pudo rendirse.");
                return RedirectToAction(nameof(AttackBoard), new { gameId = model.GameId });
            }

            ShowInfo("Te has rendido. La partida ha finalizado.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error rindiéndose");
            ShowError("No se pudo rendirse.");
            return RedirectToAction(nameof(AttackBoard), new { gameId = model.GameId });
        }
    }

    // ====================== HISTORY ======================

    [HttpGet]
    public async Task<IActionResult> History()
    {
        var userId = _currentUserService.UserId!;
        var pagedResult = await _battleshipService.GetGameHistoryAsync(userId);
        var statsResult = await _battleshipService.GetStatsAsync(userId);

        var list = pagedResult.Items.ToList();
        var items = new List<GameHistoryItemViewModel>(list.Count);
        foreach (var d in list) items.Add(MapToGameListItemToHistory(d, userId));
        var stats = statsResult.IsSuccess ? MapToGameStats(statsResult.Value!) : new GameStatsViewModel();

        var vm = new GameDetailViewModel
        {
            History = items,
            Stats = stats
        };

        await this.PopulateMenuCountersAsync(_currentUserService, _friendRequestService, _notificationService);
        return View("History/Index", vm);
    }

    // ====================== RESULT ======================

    [HttpGet]
    public async Task<IActionResult> Result(long gameId)
    {
        var userId = _currentUserService.UserId!;
        var result = await _battleshipService.GetGameResultAsync(userId, gameId);

        if (!result.IsSuccess)
        {
            ShowError(result.Error?.Message ?? "No se pudo cargar el resultado.");
            return RedirectToAction(nameof(History));
        }

        var dto = result.Value!;
        var vm = new GameResultViewModel
        {
            GameId = dto.GameId,
            OpponentId = dto.OpponentId,
            OpponentName = dto.OpponentName,
            Result = dto.Result,
            Winner = dto.Winner,
            DurationHours = dto.DurationHours,
            MyAttackBoard = ConvertToCellArray(dto.MyAttackBoard.Grid),
            OpponentAttackBoard = ConvertToCellArray(dto.OpponentAttackBoard.Grid),
            MyPlacementBoard = ConvertToCellArray(dto.MyPlacementBoard.Ships)
        };

        return View("History/Result", vm);
    }

    [HttpGet]
    public async Task<IActionResult> OpponentBoard(long gameId)
    {
        var userId = _currentUserService.UserId!;
        var result = await _battleshipService.GetOpponentAttackBoardAsync(userId, gameId);

        if (!result.IsSuccess)
        {
            ShowError("No se pudo cargar el tablero del oponente.");
            return RedirectToAction(nameof(Result), new { gameId });
        }

        var vm = new OpponentAttackBoardViewModel
        {
            GameId = gameId,
            Board = ConvertToCellArray(result.Value!.Grid)
        };

        return View("History/OpponentBoard", vm);
    }

    [HttpGet]
    public async Task<IActionResult> MyPlacementBoard(long gameId)
    {
        var userId = _currentUserService.UserId!;
        var placementResult = await _battleshipService.GetMyPlacementBoardAsync(userId, gameId);
        var attackResult = await _battleshipService.GetMyAttackBoardAsync(userId, gameId);

        if (!placementResult.IsSuccess)
        {
            ShowError("No se pudo cargar el tablero.");
            return RedirectToAction(nameof(Result), new { gameId });
        }

        var board = new CellViewModel[12, 12];
        for (var r = 0; r < 12; r++)
        for (var c = 0; c < 12; c++)
            board[r, c] = new CellViewModel { X = c, Y = r, State = BoardCellState.Empty };

        foreach (var ship in placementResult.Value!.Ships)
        {
            foreach (var cell in ship.OccupiedCells)
            {
                var x = cell[0];
                var y = cell[1];
                if (x >= 0 && x < 12 && y >= 0 && y < 12)
                    board[y, x] = new CellViewModel
                    {
                        X = x,
                        Y = y,
                        State = ship.IsSunk ? BoardCellState.Sunk : BoardCellState.Ship,
                        ShipId = ship.Id,
                        ShipSize = (int)ship.Size
                    };
            }
        }

        if (attackResult.IsSuccess)
        {
            for (var r = 0; r < 12; r++)
            for (var c = 0; c < 12; c++)
            {
                if (attackResult.Value.Grid[c, r] == BoardCellState.Hit ||
                    attackResult.Value.Grid[c, r] == BoardCellState.Sunk)
                    board[r, c].State = attackResult.Value.Grid[c, r];
            }
        }

        var vm = new MyPlacementBoardViewModel { GameId = gameId, Board = board };
        return View("History/MyPlacementBoard", vm);
    }

    private async Task<List<FriendListItemViewModel>> GetAllFriendsAsync(string userId)
    {
        var paged = await _friendshipService.GetFriendsAsync(userId, page: 1, pageSize: 500);
        return paged.Items.Select(MapToFriendListItem).ToList();
    }

    // ====================== PRIVATE HELPERS ======================

    private static ActiveGameListItemViewModel MapToActiveGameListItem(GameListItemDto dto, string currentUserId)
    {
        return new ActiveGameListItemViewModel
        {
            GameId = dto.Id,
            OpponentId = dto.OpponentId,
            OpponentName = dto.OpponentName,
            OpponentProfilePicture = null,
            Status = dto.Status,
            StartedAt = dto.StartedAt.UtcDateTime,
            HoursElapsed = dto.Duration.HasValue ? dto.Duration.Value.TotalHours : 0,
            CurrentTurnUserId = dto.CurrentTurnUserId,
            CurrentUserId = currentUserId
        };
    }

    private static GameHistoryItemViewModel MapToGameHistoryItem(GameHistoryDto dto)
    {
        return new GameHistoryItemViewModel
        {
            GameId = dto.GameId,
            OpponentId = dto.OpponentId,
            OpponentName = dto.OpponentName,
            OpponentProfilePicture = dto.OpponentProfilePicture,
            StartedAt = dto.StartedAt.UtcDateTime,
            FinishedAt = dto.FinishedAt.UtcDateTime,
            DurationHours = dto.Duration.TotalHours,
            Result = dto.IsWon ? "Ganada" : "Perdida",
            Winner = dto.Winner
        };
    }

    private GameHistoryItemViewModel MapToGameListItemToHistory(GameListItemDto dto, string currentUserId)
    {
        var isWon = dto.WinnerId == currentUserId;
        var duration = dto.Duration ?? TimeSpan.Zero;
        var winner = dto.WinnerId == null
            ? "Empate"
            : isWon
                ? "Yo"
                : dto.OpponentName;

        return new GameHistoryItemViewModel
        {
            GameId = dto.Id,
            OpponentId = dto.OpponentId,
            OpponentName = dto.OpponentName,
            OpponentProfilePicture = null,
            StartedAt = dto.StartedAt.UtcDateTime,
            FinishedAt = (dto.FinishedAt ?? dto.StartedAt).UtcDateTime,
            DurationHours = duration.TotalHours,
            Result = isWon ? "Ganada" : "Perdida",
            Winner = winner
        };
    }

    private static GameStatsViewModel MapToGameStats(GameStatsDto dto)
    {
        return new GameStatsViewModel
        {
            TotalGames = dto.TotalGames,
            WonGames = dto.WonGames,
            LostGames = dto.LostGames
        };
    }

    private static AvailableOpponentViewModel MapToAvailableOpponent(FriendListItemViewModel friend)
    {
        return new AvailableOpponentViewModel
        {
            UserId = friend.FriendId,
            FullName = friend.FriendName,
            UserName = friend.FriendUserName,
            ProfilePicturePath = friend.FriendProfilePicturePath
        };
    }

    private static FriendListItemViewModel MapToFriendListItem(LinkUpPro.Application.DTOs.Friendship.Responses.FriendListItemDto dto)
    {
        return new FriendListItemViewModel
        {
            FriendId = dto.FriendId,
            FriendName = dto.FriendName,
            FriendUserName = dto.FriendUserName,
            FriendProfilePicturePath = dto.FriendProfilePicturePath,
            CommonFriendsCount = dto.CommonFriendsCount
        };
    }

    private static List<ShipToPlaceViewModel> BuildShipsToPlace(ShipPlacementDto[] placedShips)
    {
        var required = new[] { 5, 4, 3, 3, 2 };
        var placedSizes = placedShips.Select(s => (int)s.Size).ToList();

        var result = new List<ShipToPlaceViewModel>();
        foreach (var size in required)
        {
            var placedCount = placedSizes.Count(s => s == size);
            var totalCount = required.Count(s => s == size);
            for (var i = 1; i <= totalCount; i++)
            {
                var isPlaced = i <= placedCount;
                var name = ShipToPlaceViewModel.GetName(size, i);
                result.Add(new ShipToPlaceViewModel
                {
                    Size = size,
                    Index = i,
                    Label = totalCount > 1 ? $"{name} {i}" : name,
                    DisplayName = name,
                    IsPlaced = isPlaced
                });
            }
        }

        return result.Where(s => !s.IsPlaced).ToList();
    }

    private static CellViewModel[,] BuildBoardFromPlacedShips(List<PlacedShipViewModel> placedShips)
    {
        var board = new CellViewModel[12, 12];
        for (var r = 0; r < 12; r++)
        for (var c = 0; c < 12; c++)
            board[r, c] = new CellViewModel { X = c, Y = r, State = BoardCellState.Empty };

        foreach (var ship in placedShips)
        {
            foreach (var cell in ship.OccupiedCells)
            {
                if (cell.X >= 0 && cell.X < 12 && cell.Y >= 0 && cell.Y < 12)
                    board[cell.Y, cell.X] = new CellViewModel
                    {
                        X = cell.X,
                        Y = cell.Y,
                        State = BoardCellState.Ship,
                        ShipId = ship.ShipId,
                        ShipSize = ship.Size
                    };
            }
        }
        return board;
    }

    private static CellViewModel[,] BuildFullPlacementBoard(PlacementBoardDto placement, AttackBoardDto? opponentAttacks)
    {
        var board = BuildBoardFromPlacedShips(placement.Ships.Select(MapToPlacedShip).ToList());
        if (opponentAttacks is null) return board;
        for (var r = 0; r < 12; r++)
        for (var c = 0; c < 12; c++)
        {
            if (opponentAttacks.Grid[c, r] == BoardCellState.Hit ||
                opponentAttacks.Grid[c, r] == BoardCellState.Sunk)
                board[r, c].State = opponentAttacks.Grid[c, r];
        }
        return board;
    }

    private static PlacedShipViewModel MapToPlacedShip(ShipPlacementDto dto)
    {
        return new PlacedShipViewModel
        {
            ShipId = dto.Id,
            Size = (int)dto.Size,
            StartX = dto.StartX,
            StartY = dto.StartY,
            Direction = dto.Direction,
            IsSunk = dto.IsSunk,
            OccupiedCells = dto.OccupiedCells.Select(c => new CellViewModel
            {
                X = c[0],
                Y = c[1],
                State = dto.IsSunk ? BoardCellState.Sunk : BoardCellState.Ship,
                ShipId = dto.Id,
                ShipSize = (int)dto.Size
            }).ToList()
        };
    }

    private static CellViewModel[,] ConvertToCellArray(BoardCellState[,] grid)
    {
        var board = new CellViewModel[12, 12];
        for (var r = 0; r < 12; r++)
        for (var c = 0; c < 12; c++)
            board[r, c] = new CellViewModel { X = c, Y = r, State = grid[c, r] };
        return board;
    }

    private static CellViewModel[,] ConvertToCellArray(ShipPlacementDto[] ships)
    {
        var board = new CellViewModel[12, 12];
        for (var r = 0; r < 12; r++)
        for (var c = 0; c < 12; c++)
            board[r, c] = new CellViewModel { X = c, Y = r, State = BoardCellState.Empty };

        foreach (var ship in ships)
        {
            foreach (var cell in ship.OccupiedCells)
            {
                var x = cell[0];
                var y = cell[1];
                if (x >= 0 && x < 12 && y >= 0 && y < 12)
                    board[y, x].State = ship.IsSunk ? BoardCellState.Sunk : BoardCellState.Ship;
            }
        }

        return board;
    }
}
