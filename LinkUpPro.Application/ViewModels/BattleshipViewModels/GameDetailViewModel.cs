namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel principal de la pantalla "Battleship" (listado de partidas).
/// Combina partidas activas, historial y estadísticas.
/// </summary>
public class GameDetailViewModel
{
    public List<ActiveGameListItemViewModel> ActiveGames { get; set; } = [];
    public List<GameHistoryItemViewModel> History { get; set; } = [];
    public GameStatsViewModel Stats { get; set; } = new();
}
