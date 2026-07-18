using LinkUpPro.Domain.Common;

namespace LinkUpPro.Tests.Infrastructure;

/// <summary>
/// Implementación de IDateTimeProvider para pruebas con InMemory.
/// Retorna una fecha UTC fija para garantizar que los tests sean deterministas.
/// </summary>
public sealed class TestDateTimeProvider : IDateTimeProvider
{
    public static readonly DateTimeOffset FixedUtcNow = new(2026, 6, 21, 12, 0, 0, TimeSpan.Zero);

    public DateTimeOffset UtcNow => FixedUtcNow;
}
