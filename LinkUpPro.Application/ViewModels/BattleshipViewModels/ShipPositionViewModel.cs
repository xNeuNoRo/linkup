using System.ComponentModel.DataAnnotations;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para la pantalla de selección de celda inicial (tablero 12x12).
/// El usuario hace click en una celda y la posición se envía al siguiente paso.
/// </summary>
public class ShipPositionViewModel
{
    [Required]
    [Display(Name = "Identificador de partida")]
    public long GameId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un tamaño de barco.")]
    [MustBeInFleetSizes]
    [Display(Name = "Tamaño del barco")]
    public int ShipSize { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una celda en el tablero.")]
    [Range(0, DomainConstants.BoardSize - 1,
        ErrorMessage = "La coordenada X debe estar entre 0 y 11.")]
    [Display(Name = "Coordenada X")]
    public int SelectedX { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una celda en el tablero.")]
    [Range(0, DomainConstants.BoardSize - 1,
        ErrorMessage = "La coordenada Y debe estar entre 0 y 11.")]
    [Display(Name = "Coordenada Y")]
    public int SelectedY { get; set; }

    /// <summary>
    /// Matriz 12x12 de celdas para renderizar el tablero.
    /// </summary>
    public CellViewModel[,] Board { get; set; } = new CellViewModel[DomainConstants.BoardSize, DomainConstants.BoardSize];
}
