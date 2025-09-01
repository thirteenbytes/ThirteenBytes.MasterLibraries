namespace FinanceExample.Application.Contracts.Models
{
    public sealed record EventSummary(
        Guid EventId,
        string EventType,
        DateTime Timestamp);
}