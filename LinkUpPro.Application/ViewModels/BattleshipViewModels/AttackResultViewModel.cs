namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel con el resultado de un ataque (para actualizar el tablero tras el POST).
/// </summary>
public class AttackResultViewModel
{
    public bool IsHit { get; set; }
    public bool IsSunk { get; set; }
    public int? SunkShipSize { get; set; }
    public bool IsGameOver { get; set; }
    public string? WinnerId { get; set; }
    public bool TurnChanged { get; set; }
    public string? NextTurnUserId { get; set; }
}
