namespace LinkUpPro.Application.ViewModels.CommentViewModels;

/// <summary>
/// ViewModel para mostrar un comentario o respuesta en la UI.
/// Estructura recursiva para soportar respuestas anidadas (hilos de conversación).
/// </summary>
public class CommentViewModel
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public long? ParentCommentId { get; set; }

    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorProfilePicture { get; set; }

    public string Content { get; set; } = string.Empty;
    public bool IsEdited { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int RepliesCount { get; set; }

    public bool HasMoreReplies { get; set; }

    /// <summary>
    /// Respuestas anidadas a este comentario.
    /// </summary>
    public List<CommentViewModel> Replies { get; set; } = [];

    /// <summary>
    /// Indica si el comentario fue eliminado (mostrar "Este comentario fue eliminado.").
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Indica si el comentario pertenece al usuario autenticado (para mostrar botones Editar/Eliminar).
    /// </summary>
    public bool IsOwn { get; set; }

    /// <summary>
    /// Indica si la publicación permite responder (AllowComments).
    /// </summary>
    public bool CanReply { get; set; } = true;
}
