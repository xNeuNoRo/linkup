using System.Linq.Expressions;

namespace LinkUpPro.Domain.Common;

/// <summary>
/// Encapsula las opciones de consulta para repositorios, incluyendo filtros,
/// ordenamiento, paginación e inclusión de relaciones.
/// </summary>
/// <typeparam name="T">El tipo de la entidad siendo consultada.</typeparam>
public sealed class QueryOptions<T>
{
    public Expression<Func<T, bool>>? Filter { get; set; }

    public List<Expression<Func<T, object>>> Includes { get; set; } = [];

    public Func<IQueryable<T>, IOrderedQueryable<T>>? OrderBy { get; set; }

    public int? Skip { get; set; }

    public int? Take { get; set; }

    public bool IsTracking { get; set; }
}
