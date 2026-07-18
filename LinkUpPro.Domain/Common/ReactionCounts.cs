namespace LinkUpPro.Domain.Common;

/// <summary>
/// Representa los conteos de reacciones (me gusta y no me gusta) para una publicación o comentario.
/// </summary>
public sealed record ReactionCounts(int Likes, int Dislikes)
{
    public int Total => Likes + Dislikes;
}
