using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para el tablero de ataque del oponente (vista en el resultado de partida).
/// </summary>
public class OpponentAttackBoardViewModel
{
    public long GameId { get; set; }
    public string OpponentName { get; set; } = string.Empty;
    public CellViewModel[,] Board { get; set; } = new CellViewModel[DomainConstants.BoardSize, DomainConstants.BoardSize];
}
