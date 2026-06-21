using System.Linq.Expressions;

namespace LinkUpPro.Domain.Common;

/// <summary>
/// Encapsulates repository query options without coupling the Domain layer to persistence details.
/// </summary>
/// <typeparam name="T">Entity type being queried.</typeparam>
public sealed class QueryOptions<T>
{
    public Expression<Func<T, bool>>? Filter { get; set; }

    public List<Expression<Func<T, object>>> Includes { get; set; } = [];

    public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; set; }

    public int? Skip { get; set; }

    public int? Take { get; set; }

    public bool IsTracking { get; set; }
}
