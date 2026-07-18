using LinkUpPro.Domain.Enums;

namespace LinkUpPro.Application.ViewModels.ReactionViewModels;

/// <summary>
/// ViewModel para mostrar los contadores de reacciones de una publicación
/// y la reacción actual del usuario (si tiene).
/// </summary>
public class ReactionCountsViewModel
{
    public int LikesCount { get; set; }
    public int DislikesCount { get; set; }

    /// <summary>
    /// Reacción actual del usuario autenticado (null = sin reacción).
    /// </summary>
    public ReactionType? CurrentUserReaction { get; set; }

    public bool HasReacted => CurrentUserReaction.HasValue;
}
