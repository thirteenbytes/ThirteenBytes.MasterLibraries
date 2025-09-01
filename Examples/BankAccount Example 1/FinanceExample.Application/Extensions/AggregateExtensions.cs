using ThirteenBytes.DDDPatterns.Primitives.Abstractions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;

namespace FinanceExample.Application.Extensions
{
    public static class AggregateExtensions
    {
        /// <summary>
        /// Saves uncommitted events from an aggregate to the event store and clears them.
        /// </summary>
        public static async Task SaveEventsAsync<TId, TValue>(
            this AggregateRoot<TId, TValue> aggregate,
            IEventStore eventStore,
            int expectedVersion,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>
        {
            var events = aggregate.GetUncommittedEvents();
            if (events.Any())
            {
                await eventStore.AppendEventsAsync<TId, TValue>(
                    aggregate.Id,
                    events,
                    expectedVersion,
                    cancellationToken);
                
                aggregate.ClearDomainEvents();
            }
        }

        /// <summary>
        /// Saves uncommitted events for a new aggregate (version 0).
        /// </summary>
        public static async Task SaveNewAggregateEventsAsync<TId, TValue>(
            this AggregateRoot<TId, TValue> aggregate,
            IEventStore eventStore,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>
        {
            await aggregate.SaveEventsAsync(eventStore, 0, cancellationToken);
        }
    }
}