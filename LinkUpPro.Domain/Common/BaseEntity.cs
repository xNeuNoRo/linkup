namespace LinkUpPro.Domain.Common;

/// <summary>
/// Base class for entities with generic identifiers and audit metadata.
/// </summary>
/// <typeparam name="TId">Entity identifier type.</typeparam>
public abstract class BaseEntity<TId> : IAuditableEntity
{
    public TId Id { get; protected set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>
/// Base class for entities with long identifiers.
/// </summary>
public abstract class BaseEntity : BaseEntity<long>;
