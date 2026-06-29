namespace LinkUpPro.Application.ViewModels.BattleshipViewModels;

/// <summary>
/// ViewModel para un barco pendiente de colocar.
/// </summary>
public class ShipToPlaceViewModel
{
    /// <summary>
    /// Tamaño del barco (2, 3, 4 o 5).
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// Etiqueta legible (ej. "Barco de 5", "Barco de 3 (1)").
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Índice del barco cuando hay varios del mismo tamaño (1, 2, etc.).
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Indica si ya fue colocado.
    /// </summary>
    public bool IsPlaced { get; set; }
}
