using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para una celda individual del tablero de Battleship.
/// Se usa tanto en el tablero de ataque como en el de posicionamiento.
/// </summary>
public class CellViewModel
{
    public int X { get; set; }
    public int Y { get; set; }

    /// <summary>
    /// Estado visual de la celda:
    /// - Empty: sin contenido (por defecto)
    /// - Ship: ocupada por un barco (fase de posicionamiento, vista propia)
    /// - Hit: atacado y había barco (rojo, fase de ataque)
    /// - Miss: atacado y no había barco (verde, fase de ataque)
    /// - Sunk: barco hundido completo (fase de ataque, solo visible si IsGameOver o en resultado)
    /// </summary>
    public BoardCellState State { get; set; } = BoardCellState.Empty;

    /// <summary>
    /// Indica si la celda es clickeable en el contexto actual.
    /// </summary>
    public bool IsSelectable { get; set; }

    /// <summary>
    /// Tamaño del barco si la celda está ocupada (para colorear el barco entero en el placement).
    /// </summary>
    public int? ShipSize { get; set; }

    /// <summary>
    /// ID del barco (si la celda está ocupada y hundida).
    /// </summary>
    public long? ShipId { get; set; }
}
