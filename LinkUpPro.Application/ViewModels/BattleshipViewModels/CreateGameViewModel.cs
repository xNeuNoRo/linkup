using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para la pantalla "Crear nueva partida de Battleship".
/// Muestra los amigos disponibles (sin partida activa con el usuario) y permite seleccionar uno.
/// </summary>
public class CreateGameViewModel
{
    [Required(ErrorMessage = "Debe seleccionar un amigo para iniciar la partida.")]
    [Display(Name = "Amigo seleccionado")]
    public string SelectedOpponentId { get; set; } = string.Empty;

    [Display(Name = "Buscar amigo")]
    public string? SearchText { get; set; }

    public List<AvailableOpponentViewModel> AvailableOpponents { get; set; } = [];
}
