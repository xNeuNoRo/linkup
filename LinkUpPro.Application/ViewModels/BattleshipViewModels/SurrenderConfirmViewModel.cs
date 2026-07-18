using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para la pantalla de confirmación de rendición.
/// Muestra el nombre del oponente y permite al usuario confirmar o cancelar.
/// </summary>
public class SurrenderConfirmViewModel
{
    [Required]
    [Display(Name = "Identificador de partida")]
    public long GameId { get; set; }

    public string OpponentName { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje informativo a mostrar.
    /// </summary>
    public string Message { get; set; } = "¿Está seguro que desea rendirse?";
}
