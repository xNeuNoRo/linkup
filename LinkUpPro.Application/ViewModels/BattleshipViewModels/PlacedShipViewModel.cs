using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para un barco ya colocado (para mostrarlo en el tablero).
/// </summary>
public class PlacedShipViewModel
{
    public long ShipId { get; set; }
    public int Size { get; set; }
    public int StartX { get; set; }
    public int StartY { get; set; }
    public ShipDirection Direction { get; set; }
    public bool IsSunk { get; set; }

    /// <summary>
    /// Celdas que ocupa el barco (calculadas a partir de StartX, StartY, Size, Direction).
    /// </summary>
    public List<CellViewModel> OccupiedCells { get; set; } = [];
}
