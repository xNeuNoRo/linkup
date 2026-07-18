using System.ComponentModel.DataAnnotations;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para el tablero de ataque (Fase 2).
/// Muestra el tablero con los ataques realizados por el usuario y permite atacar nuevas celdas.
/// </summary>
public class AttackBoardViewModel
{
    [Required]
    [Display(Name = "Identificador de partida")]
    public long GameId { get; set; }

    public string CurrentUserId { get; set; } = string.Empty;
    public string OpponentId { get; set; } = string.Empty;
    public string OpponentName { get; set; } = string.Empty;

    public bool IsMyTurn { get; set; }
    public bool IsGameOver { get; set; }
    public string? WinnerId { get; set; }
    public string? CurrentTurnUserId { get; set; }

    /// <summary>
    /// Matriz 12x12 de celdas del tablero de ataque.
    /// Estados: Empty (sin atacar), Hit (rojo), Miss (verde).
    /// </summary>
    public CellViewModel[,] Board { get; set; } = new CellViewModel[DomainConstants.BoardSize, DomainConstants.BoardSize];

    /// <summary>
    /// Tablero de posicionamiento del usuario (para el botón "Ver mi tablero").
    /// </summary>
    public CellViewModel[,] MyPlacementBoard { get; set; } = new CellViewModel[DomainConstants.BoardSize, DomainConstants.BoardSize];

    /// <summary>
    /// Coordenadas del último ataque enviado (para resaltarlo).
    /// </summary>
    public int? LastAttackX { get; set; }
    public int? LastAttackY { get; set; }

    /// <summary>
    /// Mensaje a mostrar debajo del tablero (ej. "Es turno del jugador X de atacar", "Tu turno").
    /// </summary>
    public string? TurnMessage { get; set; }

    /// <summary>
    /// Horas transcurridas desde la asignación del turno actual (para la regla de abandono de 48h).
    /// </summary>
    public double HoursSinceTurnAssigned { get; set; }
}
