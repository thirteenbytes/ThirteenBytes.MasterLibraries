using System.Text.Json;

namespace FinanceExample.Application.Contracts.Models
{
    public sealed class DynamicEventDetail
    {
        public Guid EventId { get; init; }
        public string EventType { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
        public JsonElement EventData { get; init; }
    }
}