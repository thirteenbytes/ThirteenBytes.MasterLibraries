using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;

namespace FinanceExample.Infrastructure.InMemory.Models
{
    // Internal model for tracking event metadata in memory
    internal sealed class InMemoryStoredEvent
    {
        public Guid EventId { get; init; }
        public string EventType { get; init; } = string.Empty;
        public IDomainEvent EventData { get; init; } = null!;
        public DateTime Timestamp { get; init; }
        public int Version { get; init; }
    }
}