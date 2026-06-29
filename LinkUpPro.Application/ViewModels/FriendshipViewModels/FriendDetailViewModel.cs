using LinkUpPro.Application.ViewModels.PostViewModels;
using LinkUpPro.Application.ViewModels.Shared;

namespace LinkUpPro.Application.ViewModels.FriendshipViewModels;

/// <summary>
/// ViewModel principal de la pantalla "Amigos".
/// Combina resumen, publicaciones de amigos, listado de amigos y filtros.
/// </summary>
public class FriendDetailViewModel : BaseViewModel
{
    /// <summary>
    /// Información del amigo (perfil).
    /// </summary>
    public FriendListItemViewModel Friend { get; set; } = new();

    /// <summary>
    /// Resumen de amistades del usuario autenticado.
    /// </summary>
    public FriendshipSummaryViewModel Summary { get; set; } = new();

    /// <summary>
    /// Publicaciones visibles del amigo (Solo amigos, activas).
    /// </summary>
    public PagedResultViewModel<PostListItemViewModel> Posts { get; set; } = new();

    /// <summary>
    /// Filtros de búsqueda de publicaciones.
    /// </summary>
    public PostFilterViewModel Filters { get; set; } = new();

    /// <summary>
    /// Buscador de amigos en el listado.
    /// </summary>
    public FriendSearchViewModel Search { get; set; } = new();

    /// <summary>
    /// Resultado de la búsqueda actual.
    /// </summary>
    public List<FriendListItemViewModel> Friends { get; set; } = [];
}
