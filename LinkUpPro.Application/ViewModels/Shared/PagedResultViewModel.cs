namespace LinkUpPro.Application.ViewModels.Shared;

/// <summary>
/// ViewModel genérico para representar resultados paginados en listas.
/// </summary>
public class PagedResultViewModel<T>
{
    /// <summary>
    /// Elementos de la página actual.
    /// </summary>
    public List<T> Items { get; set; } = [];

    /// <summary>
    /// Número de la página actual (base 1).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Cantidad de elementos por página.
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Total de elementos que cumplen con los filtros aplicados.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Total de páginas disponibles.
    /// </summary>
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalItems / PageSize);

    /// <summary>
    /// Indica si existe una página anterior.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Indica si existe una página siguiente.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}
