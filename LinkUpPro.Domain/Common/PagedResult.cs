namespace LinkUpPro.Domain.Common;

/// <summary>
/// Represents a paginated set of items returned by read use cases.
/// </summary>
/// <typeparam name="T">Item type.</typeparam>
public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}
