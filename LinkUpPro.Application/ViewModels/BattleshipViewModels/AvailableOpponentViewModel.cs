namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para un amigo disponible para retar a Battleship.
/// Excluye amigos con los que ya hay partida activa.
/// </summary>
public class AvailableOpponentViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePicturePath { get; set; }
}
