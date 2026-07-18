using LinkUpPro.Domain.Common;

namespace LinkUpPro.Infrastructure.Persistence.Providers;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
