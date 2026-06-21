namespace LinkUpPro.Domain.Common;

/// <summary>
/// Aggregated post reaction counters.
/// </summary>
public sealed record ReactionCounts(int Likes, int Dislikes)
{
    public int Total => Likes + Dislikes;
}
