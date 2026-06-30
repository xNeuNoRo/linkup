using System.ComponentModel.DataAnnotations;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;
using LinkUpPro.Domain.Common;

namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel principal de la pantalla de selección de barcos a colocar (Fase 1).
/// Muestra los barcos pendientes y los ya colocados.
/// </summary>
public class ShipPlacementViewModel
{
    [Required]
    [Display(Name = "Identificador de partida")]
    public long GameId { get; set; }

    /// <summary>
    /// Tamaño del barco seleccionado para colocar (se envía en el POST).
    /// </summary>
    [Required(ErrorMessage = "Debe seleccionar un barco para posicionar.")]
    [MustBeInFleetSizes]
    [Display(Name = "Tamaño del barco seleccionado")]
    public int? SelectedShipSize { get; set; }

    /// <summary>
    /// Barcos que aún no han sido colocados.
    /// </summary>
    public List<ShipToPlaceViewModel> ShipsToPlace { get; set; } = [];

    /// <summary>
    /// Barcos ya colocados (para mostrarlos visualmente en el tablero).
    /// </summary>
    public List<PlacedShipViewModel> PlacedShips { get; set; } = [];

    /// <summary>
    /// Indica si el oponente ya terminó de colocar sus barcos.
    /// </summary>
    public bool OpponentHasFinishedPlacement { get; set; }

    /// <summary>
    /// Indica si el usuario logueado ya terminó de colocar todos sus barcos.
    /// </summary>
    public bool AllShipsPlaced => ShipsToPlace.Count == 0;

    /// <summary>
    /// Mensaje informativo (por ejemplo: "El otro jugador aún no termina de configurar sus barcos").
    /// </summary>
    public string? InfoMessage { get; set; }

    /// <summary>
    /// Matriz 12x12 de celdas para renderizar el tablero del usuario (vista previa).
    /// </summary>
    public CellViewModel[,] Board { get; set; } = new CellViewModel[DomainConstants.BoardSize, DomainConstants.BoardSize];

    /// <summary>
    /// Coordenada X inicial del barco seleccionado (fase de selección de dirección).
    /// </summary>
    public int? StartX { get; set; }

    /// <summary>
    /// Coordenada Y inicial del barco seleccionado (fase de selección de dirección).
    /// </summary>
    public int? StartY { get; set; }

    /// <summary>
    /// Etiqueta legible de la celda inicial (ej. "A5").
    /// </summary>
    public string? StartCellLabel { get; set; }

    /// <summary>
    /// Nombre legible del barco seleccionado (ej. "Barco de 5").
    /// </summary>
    public string? SelectedShipDisplayName { get; set; }
}
