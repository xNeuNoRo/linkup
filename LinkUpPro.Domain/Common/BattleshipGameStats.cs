namespace LinkUpPro.Domain.Common;

/// <summary>
/// Estadísticas calculadas de Battleship para un jugador.
/// </summary>
public sealed record BattleshipGameStats(int TotalGames, int WonGames, int LostGames)
{
    public int FinishedGames => WonGames + LostGames;
}
