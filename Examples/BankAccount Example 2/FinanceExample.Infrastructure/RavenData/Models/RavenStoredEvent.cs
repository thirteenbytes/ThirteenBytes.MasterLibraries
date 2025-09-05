using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;

namespace FinanceExample.Infrastructure.RavenData.Models
{
    public class RavenStoredEvent
    {
        public Guid EventId { get; set; }
        public string EventType { get; set; } = null!;
        public IDomainEvent EventData { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public int Version { get; set; }
    }
}