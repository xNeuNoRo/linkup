namespace LinkUpPro.Domain.Common;

/// <summary>
/// Representa un resultado paginado de una consulta.
/// </summary>
/// <typeparam name="T">El tipo de los elementos.</typeparam>
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
