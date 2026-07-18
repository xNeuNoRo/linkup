using LinkUpPro.Application.ViewModels.CommentViewModels;

namespace LinkUpPro.Application.ViewModels.PostViewModels;

/// <summary>
/// ViewModel para el detalle de una publicación.
/// Incluye los comentarios completos (árbol anidado).
/// Se usa cuando el usuario accede vía notificación o URL directa.
/// </summary>
public class PostDetailViewModel : PostListItemViewModel
{
    /// <summary>
    /// Árbol completo de comentarios con sus respuestas anidadas.
    /// </summary>
    public List<CommentViewModel> CommentTree { get; set; } = [];
}
