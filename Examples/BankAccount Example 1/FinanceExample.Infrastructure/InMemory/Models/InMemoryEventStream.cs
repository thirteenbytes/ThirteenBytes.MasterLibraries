namespace FinanceExample.Infrastructure.InMemory.Models
{
    // Internal event stream model for in-memory storage
    internal sealed class InMemoryEventStream
    {
        public string AggregateId { get; init; } = string.Empty;
        public string AggregateType { get; init; } = string.Empty;
        public int Version { get; set; }
        public List<InMemoryStoredEvent> Events { get; init; } = new();
    }
}