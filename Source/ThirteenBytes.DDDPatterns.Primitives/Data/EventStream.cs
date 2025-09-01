using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;

namespace ThirteenBytes.DDDPatterns.Primitives.Data
{
    public sealed class EventStream
    {
        public string AggregateId { get; init; } = string.Empty;
        public string AggregateType { get; init; } = string.Empty;
        public int Version { get; set; }
        public List<StoredEvent> Events { get; init; } = new();
    }
}