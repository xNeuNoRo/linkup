using LinkUpPro.Application.ViewModels.CommentViewModels;
using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.ViewModels.PostViewModels;

/// <summary>
/// ViewModel para mostrar un item de publicación en listados (Home, Amigos, Perfil, Búsqueda).
/// Mapea desde PostListItemDto.
/// </summary>
public class PostListItemViewModel
{
    public long Id { get; set; }

    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorUserName { get; set; } = string.Empty;
    public string? AuthorProfilePicture { get; set; }

    public string Content { get; set; } = string.Empty;

    public PostContentType ContentType { get; set; }
    public string? MediaPath { get; set; }

    public PrivacyLevel Privacy { get; set; }
    public bool AllowComments { get; set; }
    public bool IsEdited { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }

    /// <summary>
    /// Reacción actual del usuario autenticado (null = sin reacción, 1 = Like, 2 = Dislike).
    /// </summary>
    public ReactionType? CurrentUserReaction { get; set; }

    public int CommentsCount { get; set; }

    /// <summary>
    /// Comentarios cargados (en el Home pueden venir lazy, o el detalle los trae todos).
    /// </summary>
    public List<CommentViewModel> Comments { get; set; } = [];
}
