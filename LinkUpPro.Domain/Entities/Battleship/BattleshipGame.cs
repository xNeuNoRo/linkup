using LinkUpPro.Domain.Common;
using LinkUpPro.Domain.Enums;
using LinkUpPro.Domain.ValueObjects;

namespace LinkUpPro.Domain.Entities.Battleship;

public sealed class BattleshipGame : AuditableBaseEntity<long>
{
    private BattleshipGame() { }

    public string CreatorId { get; private set; } = null!;

    public string OpponentId { get; private set; } = null!;

    public GameStatus Status { get; private set; }

    public string? CurrentTurnUserId { get; private set; }

    public string? WinnerId { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    public DateTimeOffset? TurnAssignedAt { get; private set; }

    public DateTimeOffset? FinishedAt { get; private set; }

    public static Result<BattleshipGame> Create(
        string creatorId,
        string opponentId,
        DateTimeOffset? createdAt = null
    )
    {
        var errors = ValidatePlayers(creatorId, opponentId);

        if (errors.Count > 0)
        {
            return Result<BattleshipGame>.Failure(errors);
        }

        var date = createdAt ?? DateTimeOffset.UtcNow;

        return Result<BattleshipGame>.Success(
            new BattleshipGame
            {
                CreatorId = creatorId.Trim(),
                OpponentId = opponentId.Trim(),
                Status = GameStatus.Configuring_P1,
                CurrentTurnUserId = null,
                WinnerId = null,
                StartedAt = date,
                TurnAssignedAt = null,
                FinishedAt = null,
                CreatedAt = date,
            }
        );
    }

    public Result CompletePlayerPlacement(
        string playerId,
        IReadOnlyCollection<BattleshipShip> playerShips,
        DateTimeOffset? completedAt = null
    )
    {
        ArgumentNullException.ThrowIfNull(playerShips);

        var authorization = EnsurePlayer(playerId);
        if (authorization.IsFailure)
        {
            return authorization;
        }

        var fleetValidation = ValidateFleet(playerId, playerShips);
        if (fleetValidation.IsFailure)
        {
            return fleetValidation;
        }

        var date = completedAt ?? DateTimeOffset.UtcNow;

        if (Status == GameStatus.Configuring_P1 && playerId == CreatorId)
        {
            Status = GameStatus.Configuring_P2;
            UpdatedAt = date;
            return Result.Success();
        }

        if (Status == GameStatus.Configuring_P2 && playerId == OpponentId)
        {
            Status = GameStatus.InProgress;
            CurrentTurnUserId = CreatorId;
            TurnAssignedAt = date;
            UpdatedAt = date;
            return Result.Success();
        }

        return Result.Failure(
            new DomainError(
                "Battleship.InvalidPlacementPhase",
                "No corresponde configurar barcos en esta fase de la partida."
            )
        );
    }

    public Result<BattleshipAttack> ExecuteAttack(
        string attackerId,
        Coordinates target,
        IReadOnlyCollection<BattleshipShip> opponentShips,
        IReadOnlyCollection<BattleshipAttack> existingAttacks,
        DateTimeOffset? attackedAt = null
    )
    {
        ArgumentNullException.ThrowIfNull(opponentShips);
        ArgumentNullException.ThrowIfNull(existingAttacks);

        var turnValidation = EnsureCanAttack(attackerId, target, existingAttacks);
        if (turnValidation.IsFailure)
        {
            return Result<BattleshipAttack>.Failure(turnValidation.Errors);
        }

        var opponentId = GetOpponentId(attackerId).Value;
        var targetShip = opponentShips.FirstOrDefault(ship =>
            ship.PlayerId == opponentId && ship.Occupies(target)
        );
        var isHit = targetShip is not null;
        var attackDate = attackedAt ?? DateTimeOffset.UtcNow;
        var attackResult = BattleshipAttack.Record(
            Id,
            attackerId,
            target,
            isHit,
            targetShip?.Id,
            attackDate
        );

        if (attackResult.IsFailure)
        {
            return attackResult;
        }

        var attack = attackResult.Value;
        var attacksIncludingCurrent = existingAttacks.Concat([attack]).ToArray();

        if (targetShip is not null)
        {
            targetShip.RefreshSunkState(attacksIncludingCurrent, attackDate);
        }

        if (
            opponentShips
                .Where(ship => ship.PlayerId == opponentId)
                .All(ship => ship.RefreshSunkState(attacksIncludingCurrent, attackDate))
        )
        {
            Finish(attackerId, GameStatus.Finished_Winner, attackDate);
            return Result<BattleshipAttack>.Success(attack);
        }

        CurrentTurnUserId = opponentId;
        TurnAssignedAt = attackDate;
        UpdatedAt = attackDate;

        return Result<BattleshipAttack>.Success(attack);
    }

    public Result Surrender(string surrenderingUserId, DateTimeOffset? surrenderedAt = null)
    {
        var playerValidation = EnsurePlayer(surrenderingUserId);
        if (playerValidation.IsFailure)
        {
            return playerValidation;
        }

        if (IsFinished())
        {
            return Result.Failure(
                new DomainError("Battleship.GameAlreadyFinished", "La partida ya finalizo.")
            );
        }

        var winnerId = GetOpponentId(surrenderingUserId).Value;
        Finish(winnerId, GameStatus.Finished_Winner, surrenderedAt ?? DateTimeOffset.UtcNow);

        return Result.Success();
    }

    public Result CheckAbandonment(DateTimeOffset now)
    {
        if (Status != GameStatus.InProgress || CurrentTurnUserId is null || TurnAssignedAt is null)
        {
            return Result.Success();
        }

        if (now - TurnAssignedAt.Value < DomainConstants.BattleshipTurnTimeout)
        {
            return Result.Success();
        }

        var winnerId = GetOpponentId(CurrentTurnUserId).Value;
        Finish(winnerId, GameStatus.Finished_Abandoned, now);

        return Result.Success();
    }

    public bool IsPlayerInGame(string userId) => CreatorId == userId || OpponentId == userId;

    public Result<string> GetOpponentId(string userId)
    {
        if (CreatorId == userId)
        {
            return Result<string>.Success(OpponentId);
        }

        if (OpponentId == userId)
        {
            return Result<string>.Success(CreatorId);
        }

        return Result<string>.Failure(
            new DomainError("Battleship.UserNotInGame", "El usuario no participa en esta partida.")
        );
    }

    public bool IsFinished() =>
        Status is GameStatus.Finished_Winner or GameStatus.Finished_Abandoned;

    public TimeSpan? GetTurnElapsedTime(DateTimeOffset now) =>
        TurnAssignedAt is null ? null : now - TurnAssignedAt.Value;

    private Result EnsureCanAttack(
        string attackerId,
        Coordinates target,
        IReadOnlyCollection<BattleshipAttack> existingAttacks
    )
    {
        var playerValidation = EnsurePlayer(attackerId);
        if (playerValidation.IsFailure)
        {
            return playerValidation;
        }

        if (Status != GameStatus.InProgress)
        {
            return Result.Failure(
                new DomainError(
                    "Battleship.GameNotInProgress",
                    "La partida no se encuentra en fase de ataque."
                )
            );
        }

        if (CurrentTurnUserId != attackerId)
        {
            return Result.Failure(
                new DomainError("Battleship.NotPlayerTurn", "No es su turno de atacar.")
            );
        }

        if (
            existingAttacks.Any(attack =>
                attack.AttackerId == attackerId && attack.GetTarget() == target
            )
        )
        {
            return Result.Failure(
                new DomainError("Battleship.CellAlreadyAttacked", "Ya ha atacado esta coordenada.")
            );
        }

        return Result.Success();
    }

    private Result ValidateFleet(string playerId, IReadOnlyCollection<BattleshipShip> playerShips)
    {
        if (playerShips.Count != DomainConstants.BattleshipFleetShipCount)
        {
            return Result.Failure(
                new DomainError(
                    "Battleship.InvalidFleetCount",
                    "Debe posicionar los 5 barcos requeridos."
                )
            );
        }

        if (playerShips.Any(ship => ship.PlayerId != playerId))
        {
            return Result.Failure(
                new DomainError(
                    "Battleship.InvalidFleetOwner",
                    "Todos los barcos deben pertenecer al jugador actual."
                )
            );
        }

        var actualSizes = playerShips.Select(ship => (int)ship.Size).Order().ToArray();
        var requiredSizes = DomainConstants.RequiredBattleshipFleetSizes.Order().ToArray();

        if (!actualSizes.SequenceEqual(requiredSizes))
        {
            return Result.Failure(
                new DomainError(
                    "Battleship.InvalidFleetComposition",
                    "La flota debe contener barcos de tamanos 5, 4, 3, 3 y 2."
                )
            );
        }

        var allCells = playerShips.SelectMany(ship => ship.GetOccupiedCells()).ToArray();

        if (allCells.Length != allCells.Distinct().Count())
        {
            return Result.Failure(
                new DomainError("Battleship.OverlappingShips", "Los barcos no pueden superponerse.")
            );
        }

        return Result.Success();
    }

    private Result EnsurePlayer(string userId)
    {
        if (!IsPlayerInGame(userId))
        {
            return Result.Failure(
                new DomainError(
                    "Battleship.UserNotInGame",
                    "El usuario no participa en esta partida."
                )
            );
        }

        return Result.Success();
    }

    private void Finish(string winnerId, GameStatus finishedStatus, DateTimeOffset finishedAt)
    {
        Status = finishedStatus;
        WinnerId = winnerId;
        FinishedAt = finishedAt;
        CurrentTurnUserId = null;
        UpdatedAt = finishedAt;
    }

    private static List<DomainError> ValidatePlayers(string creatorId, string opponentId)
    {
        var errors = new List<DomainError>();

        if (string.IsNullOrWhiteSpace(creatorId))
        {
            errors.Add(
                new DomainError(
                    "Battleship.CreatorRequired",
                    "El creador de la partida es requerido."
                )
            );
        }

        if (string.IsNullOrWhiteSpace(opponentId))
        {
            errors.Add(
                new DomainError(
                    "Battleship.OpponentRequired",
                    "El oponente de la partida es requerido."
                )
            );
        }

        if (!string.IsNullOrWhiteSpace(creatorId) && creatorId.Trim() == opponentId?.Trim())
        {
            errors.Add(
                new DomainError(
                    "Battleship.SelfGameNotAllowed",
                    "No puede iniciar una partida contra usted mismo."
                )
            );
        }

        return errors;
    }
}
