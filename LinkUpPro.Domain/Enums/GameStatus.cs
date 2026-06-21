namespace LinkUpPro.Domain.Enums;

/// <summary>
/// Defines the lifecycle states of a Battleship game.
/// </summary>
public enum GameStatus
{
    Configuring_P1 = 1,
    Configuring_P2 = 2,
    InProgress = 3,
    Finished_Winner = 4,
    Finished_Abandoned = 5,
}
