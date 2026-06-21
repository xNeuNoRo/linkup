using LinkUpPro.Domain.Common;

namespace LinkUpPro.Tests.Domain.Common;

public class AuditableBaseEntityTests
{
    [Fact]
    public void MarkAsDeleted_ActiveEntity_SetsDeletedAtAndUpdatedAt()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var deletedAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);

        // Act
        entity.MarkAsDeleted(deletedAt);

        // Assert
        Assert.True(entity.IsDeleted);
        Assert.Equal(deletedAt, entity.DeletedAt);
        Assert.Equal(deletedAt, entity.UpdatedAt);
    }

    [Fact]
    public void MarkAsDeleted_AlreadyDeletedEntity_DoesNotOverwriteDeletedAt()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        var firstDeletedAt = new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero);
        var secondDeletedAt = firstDeletedAt.AddHours(1);
        entity.MarkAsDeleted(firstDeletedAt);

        // Act
        entity.MarkAsDeleted(secondDeletedAt);

        // Assert
        Assert.Equal(firstDeletedAt, entity.DeletedAt);
    }

    [Fact]
    public void Restore_DeletedEntity_ClearsDeletedAtAndUpdatesTimestamp()
    {
        // Arrange
        var entity = new TestAuditableEntity();
        entity.MarkAsDeleted(new DateTimeOffset(2026, 6, 20, 12, 0, 0, TimeSpan.Zero));

        // Act
        entity.Restore();

        // Assert
        Assert.False(entity.IsDeleted);
        Assert.Null(entity.DeletedAt);
        Assert.NotNull(entity.UpdatedAt);
    }

    private sealed class TestAuditableEntity : AuditableBaseEntity<long>;
}
