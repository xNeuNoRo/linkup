namespace LinkUpPro.Domain.Common;

/// <summary>
/// Clase base para entidades con identificadores de tipo TId.
/// </summary>
/// <typeparam name="TId">El tipo de identificador de la entidad.</typeparam>
public abstract class BaseEntity<TId> : IAuditableEntity
{
    public TId Id { get; protected set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>
/// Clase base para entidades con identificadores de tipo long.
/// </summary>
public abstract class BaseEntity : BaseEntity<long>;
