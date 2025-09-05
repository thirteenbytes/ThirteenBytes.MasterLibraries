using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;

namespace FinanceExample.Infrastructure.RavenData.Models
{
    public class RavenEventStream
    {
        public string Id { get; set; } = null!;
        public string AggregateId { get; set; } = null!;
        public string AggregateType { get; set; } = null!;
        public int Version { get; set; }
        public List<RavenStoredEvent> Events { get; set; } = new();

        public static string CreateId(string aggregateType, string aggregateId)
        {
            return $"EventStreams/{aggregateType}_{aggregateId}";
        }
    }
}