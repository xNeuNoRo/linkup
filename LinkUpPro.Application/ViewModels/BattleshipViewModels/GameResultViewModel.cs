using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para la pantalla "Ver resultado" de una partida finalizada.
/// Muestra los tableros finales de ataque y de posicionamiento.
/// </summary>
public class GameResultViewModel
{
    public long GameId { get; set; }
    public string OpponentId { get; set; } = string.Empty;
    public string OpponentName { get; set; } = string.Empty;
    public string CurrentUserId { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public double DurationHours { get; set; }

    public string Result { get; set; } = string.Empty;
    public string Winner { get; set; } = string.Empty;

    /// <summary>
    /// Tablero de ataque del usuario (sus ataques contra el oponente).
    /// </summary>
    public CellViewModel[,] MyAttackBoard { get; set; } = new CellViewModel[DomainConstants.BoardSize, DomainConstants.BoardSize];

    /// <summary>
    /// Tablero de ataque del oponente (los ataques del oponente contra el usuario).
    /// </summary>
    public CellViewModel[,] OpponentAttackBoard { get; set; } = new CellViewModel[DomainConstants.BoardSize, DomainConstants.BoardSize];

    /// <summary>
    /// Tablero de posicionamiento del usuario (cómo colocó sus barcos).
    /// </summary>
    public CellViewModel[,] MyPlacementBoard { get; set; } = new CellViewModel[DomainConstants.BoardSize, DomainConstants.BoardSize];

    public bool IsWon => Result == "Ganada";
}
