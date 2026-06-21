namespace LinkUpPro.Domain.Common;

/// <summary>
/// Base class for entities that support logical deletion through DeletedAt.
/// </summary>
/// <typeparam name="TId">Entity identifier type.</typeparam>
public abstract class AuditableBaseEntity<TId> : BaseEntity<TId>
{
    public DateTimeOffset? DeletedAt { get; protected set; }

    public bool IsDeleted => DeletedAt.HasValue;

    public virtual void MarkAsDeleted(DateTimeOffset? deletedAt = null)
    {
        if (IsDeleted)
        {
            return;
        }

        DeletedAt = deletedAt ?? DateTimeOffset.UtcNow;
        UpdatedAt = DeletedAt;
    }

    public virtual void Restore()
    {
        if (!IsDeleted)
        {
            return;
        }

        DeletedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Base class for soft-deletable entities with long identifiers.
/// </summary>
public abstract class AuditableBaseEntity : AuditableBaseEntity<long>;
