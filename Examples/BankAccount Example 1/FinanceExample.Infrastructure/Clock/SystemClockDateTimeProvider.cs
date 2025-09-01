using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Clock;

namespace FinanceExample.Infrastructure.Clock
{
    internal sealed class SystemClockDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow =>
            DateTime.UtcNow;
    }
}
