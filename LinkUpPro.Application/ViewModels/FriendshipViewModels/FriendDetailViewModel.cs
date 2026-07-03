using LinkUpPro.Application.ViewModels.PostViewModels;
using LinkUpPro.Application.ViewModels.Shared;

namespace LinkUpPro.Application.ViewModels.FriendshipViewModels;

public class FriendDetailViewModel : BaseViewModel
{
    /// <summary>
    /// Información del amigo (perfil, usado en Detail).
    /// </summary>
    public FriendListItemViewModel Friend { get; set; } = new();

    /// <summary>
    /// Resumen de amistades del usuario autenticado (usado en Index).
    /// </summary>
    public FriendshipSummaryViewModel Summary { get; set; } = new();

    /// <summary>
    /// Publicaciones del amigo (usado en Detail).
    /// </summary>
    public PagedResultViewModel<PostListItemViewModel> Posts { get; set; } = new();

    /// <summary>
    /// Filtros de búsqueda de publicaciones (usado en Detail).
    /// </summary>
    public PostFilterViewModel Filters { get; set; } = new();

    /// <summary>
    /// Buscador de amigos en el listado.
    /// </summary>
    public FriendSearchViewModel Search { get; set; } = new();
}
