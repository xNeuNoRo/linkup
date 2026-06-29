using LinkUpPro.Application.ViewModels.Shared;

namespace LinkUpPro.Application.ViewModels.PostViewModels;

/// <summary>
/// ViewModel principal de la pantalla "Home".
/// Combina el formulario de creación, la lista paginada de publicaciones y los filtros.
/// Hereda de BaseViewModel para incluir contadores del menú superior.
/// </summary>
public class HomeViewModel : BaseViewModel
{
    /// <summary>
    /// Formulario de creación de publicación (parte superior).
    /// </summary>
    public CreatePostViewModel CreatePost { get; set; } = new();

    /// <summary>
    /// Filtros aplicados a la búsqueda de publicaciones.
    /// </summary>
    public PostFilterViewModel Filters { get; set; } = new();

    /// <summary>
    /// Resultado paginado de publicaciones del usuario autenticado.
    /// </summary>
    public PagedResultViewModel<PostListItemViewModel> Posts { get; set; } = new();
}
