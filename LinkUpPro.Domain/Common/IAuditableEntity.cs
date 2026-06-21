namespace LinkUpPro.Domain.Common;

/// <summary>
/// Interfaz que define las propiedades de auditoría para las entidades.
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }

    DateTimeOffset? UpdatedAt { get; set; }
}
