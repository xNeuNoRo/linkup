namespace LinkUpPro.Domain.Common;

/// <summary>
/// Defines the audit contract shared by all persisted domain entities.
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }

    DateTimeOffset? UpdatedAt { get; set; }
}
