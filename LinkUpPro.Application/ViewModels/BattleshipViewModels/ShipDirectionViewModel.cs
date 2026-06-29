using System.ComponentModel.DataAnnotations;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para la pantalla de selección de dirección del barco.
/// Recibe la celda inicial seleccionada y pide la dirección (Arriba/Abajo/Izquierda/Derecha).
/// </summary>
public class ShipDirectionViewModel
{
    [Required]
    [Display(Name = "Identificador de partida")]
    public long GameId { get; set; }

    [Required]
    [MustBeInFleetSizes]
    [Display(Name = "Tamaño del barco")]
    public int ShipSize { get; set; }

    [Required]
    [Range(0, DomainConstants.BoardSize - 1)]
    [Display(Name = "Coordenada X inicial")]
    public int StartX { get; set; }

    [Required]
    [Range(0, DomainConstants.BoardSize - 1)]
    [Display(Name = "Coordenada Y inicial")]
    public int StartY { get; set; }

    /// <summary>
    /// Dirección: 1 = Up, 2 = Down, 3 = Right, 4 = Left. Mapea a ShipDirection.
    /// </summary>
    [Required(ErrorMessage = "Debe seleccionar una dirección para el barco.")]
    [Range(1, 4, ErrorMessage = "La dirección seleccionada no es válida. Las opciones válidas son: 1 (Arriba), 2 (Abajo), 3 (Derecha), 4 (Izquierda).")]
    [Display(Name = "Dirección")]
    public int Direction { get; set; }

    /// <summary>
    /// Mensaje de error de validación de negocio (por ejemplo: "El barco quedaría fuera del tablero"
    /// o "El barco se superpondría a otro ya colocado").
    /// Se muestra encima del formulario cuando el servicio devuelve error.
    /// </summary>
    public string? BusinessErrorMessage { get; set; }
}
