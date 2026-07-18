namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para una partida finalizada en el historial.
/// </summary>
public class GameHistoryItemViewModel
{
    public long GameId { get; set; }
    public string OpponentId { get; set; } = string.Empty;
    public string OpponentName { get; set; } = string.Empty;
    public string? OpponentProfilePicture { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
    public double DurationHours { get; set; }

    /// <summary>
    /// Resultado legible: "Ganada" o "Perdida".
    /// </summary>
    public string Result { get; set; } = string.Empty;

    /// <summary>
    /// Ganador: "Yo" si ganó el usuario autenticado, sino el UserName del oponente.
    /// </summary>
    public string Winner { get; set; } = string.Empty;

    public bool IsWon => Result == "Ganada";
}
