using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para una partida activa en el listado principal de Battleship.
/// </summary>
public class ActiveGameListItemViewModel
{
    public long GameId { get; set; }
    public string OpponentId { get; set; } = string.Empty;
    public string OpponentName { get; set; } = string.Empty;
    public string? OpponentProfilePicture { get; set; }

    public GameStatus Status { get; set; }
    public DateTime StartedAt { get; set; }
    public double HoursElapsed { get; set; }

    public string? CurrentTurnUserId { get; set; }
    public string CurrentUserId { get; set; } = string.Empty;

    /// <summary>
    /// Calculado: indica si es el turno del usuario autenticado.
    /// </summary>
    public bool IsMyTurn => CurrentTurnUserId == CurrentUserId;

    /// <summary>
    /// Indica si la partida está en fase de configuración de barcos (jugador aún no ha colocado sus barcos).
    /// </summary>
    public bool IsConfiguring => Status == GameStatus.Configuring_P1 || Status == GameStatus.Configuring_P2;

    /// <summary>
    /// Indica si el usuario puede rendirse (solo en partidas InProgress).
    /// </summary>
    public bool CanSurrender => Status == GameStatus.InProgress;
}
