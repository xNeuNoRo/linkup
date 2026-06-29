using System.ComponentModel.DataAnnotations;
using LinkUpPro.Application.ViewModels.Shared.ValidationAttributes;

namespace LinkUpPro.Application.ViewModels.PostViewModels;

/// <summary>
/// ViewModel para los filtros y búsqueda de publicaciones.
/// Se usa tanto en Home (publicaciones del usuario) como en Amigos (publicaciones de amigos).
/// </summary>
public class PostFilterViewModel
{
    [Display(Name = "Texto de búsqueda")]
    public string? SearchText { get; set; }

    /// <summary>
    /// Tipo de contenido: null = Todos, 1 = Imagen, 2 = YouTube.
    /// </summary>
    [Display(Name = "Tipo de contenido")]
    public int? ContentType { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha desde")]
    [DateRange(nameof(ToDate))]
    public DateTime? FromDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Fecha hasta")]
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Estado de edición: null = Todas, true = Solo editadas, false = Solo no editadas.
    /// </summary>
    [Display(Name = "Estado de edición")]
    public bool? EditedOnly { get; set; }

    /// <summary>
    /// (Solo Amigos) Filtrar publicaciones de un amigo específico.
    /// </summary>
    [Display(Name = "Amigo")]
    public string? FriendId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El número de página debe ser mayor que cero.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
    public int PageSize { get; set; } = 20;
}
