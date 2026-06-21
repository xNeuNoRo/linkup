namespace LinkUpPro.Domain.Common;

/// <summary>
/// Aggregated Battleship statistics for a player.
/// </summary>
public sealed record BattleshipGameStats(int TotalGames, int WonGames, int LostGames)
{
    public int FinishedGames => WonGames + LostGames;
}
