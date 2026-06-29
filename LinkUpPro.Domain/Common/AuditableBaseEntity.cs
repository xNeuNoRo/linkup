namespace LinkUpPro.Domain.Common;

/// <summary>
/// Clase base para entidades que requieren seguimiento de auditoría.
/// </summary>
/// <typeparam name="TId">El tipo de identificador de la entidad.</typeparam>
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
/// Clase base para entidades con identificadores de tipo long que requieren seguimiento de auditoría.
/// </summary>
public abstract class AuditableBaseEntity : AuditableBaseEntity<long>;
