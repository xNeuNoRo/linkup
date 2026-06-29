using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para el tablero de posicionamiento del usuario (cómo colocó sus barcos).
/// Se muestra en el resultado de partida o durante la fase 1 como confirmación.
/// </summary>
public class MyPlacementBoardViewModel
{
    public long GameId { get; set; }
    public CellViewModel[,] Board { get; set; } = new CellViewModel[DomainConstants.BoardSize, DomainConstants.BoardSize];
    public List<PlacedShipViewModel> Ships { get; set; } = [];
}
