namespace LinkUpPro.Application.ViewModels.CommentViewModels;

/// <summary>
/// ViewModel para representar el árbol completo de comentarios de una publicación.
/// Solo contiene los comentarios raíz (sin padre); cada uno tiene sus respuestas anidadas.
/// </summary>
public class CommentTreeViewModel
{
    public long PostId { get; set; }
    public List<CommentViewModel> Comments { get; set; } = [];
    public int TotalCount { get; set; }
}
