namespace LinkUpPro.Domain.Enums;

/// <summary>
/// Enum que representa el estado de un juego de Battleship entre dos usuarios.
/// </summary>
public enum GameStatus
{
    Configuring_P1 = 1,
    Configuring_P2 = 2,
    InProgress = 3,
    Finished_Winner = 4,
    Finished_Abandoned = 5,
}
