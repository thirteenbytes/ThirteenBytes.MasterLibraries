using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;

namespace ThirteenBytes.DDDPatterns.Primitives.Data
{
    public sealed class StoredEvent
    {
        public Guid EventId { get; init; }
        public string EventType { get; init; } = string.Empty;
        public IDomainEvent EventData { get; init; } = null!;
        public DateTime Timestamp { get; init; }
        public int Version { get; init; }
    }
}