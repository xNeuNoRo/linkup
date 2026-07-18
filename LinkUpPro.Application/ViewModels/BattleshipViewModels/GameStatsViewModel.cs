namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para el resumen de estadísticas del historial (total, ganadas, perdidas).
/// Se muestra encima del listado de partidas finalizadas.
/// </summary>
public class GameStatsViewModel
{
    public int TotalGames { get; set; }
    public int WonGames { get; set; }
    public int LostGames { get; set; }
}
