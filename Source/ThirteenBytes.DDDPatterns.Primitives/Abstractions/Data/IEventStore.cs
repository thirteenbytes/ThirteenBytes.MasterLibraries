using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;
using ThirteenBytes.DDDPatterns.Primitives.Data;

namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data
{
    public interface IEventStore
    {
        /// <summary>
        /// Append events to the event store for a specific aggregate
        /// </summary>
        Task AppendEventsAsync<TId, TValue>(
            TId aggregateId, 
            IEnumerable<IDomainEvent> events, 
            int expectedVersion,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>;

        /// <summary>
        /// Get all events for a specific aggregate
        /// </summary>
        Task<IEnumerable<IDomainEvent>> GetEventsAsync<TId, TValue>(
            TId aggregateId,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>;

        /// <summary>
        /// Get events for a specific aggregate from a specific version
        /// </summary>
        Task<IEnumerable<IDomainEvent>> GetEventsAsync<TId, TValue>(
            TId aggregateId,
            int fromVersion,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>;

        /// <summary>
        /// Get paginated events for a specific aggregate
        /// </summary>
        Task<PagedEventResult> GetEventsPagedAsync<TId, TValue>(
            TId aggregateId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>;

        /// <summary>
        /// Get the current version of an aggregate
        /// </summary>
        Task<int> GetAggregateVersionAsync<TId, TValue>(
            TId aggregateId,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>;

        /// <summary>
        /// Check if an aggregate exists in the event store
        /// </summary>
        Task<bool> AggregateExistsAsync<TId, TValue>(
            TId aggregateId,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>;

        /// <summary>
        /// Get paginated stored events (with metadata) for a specific aggregate
        /// </summary>
        Task<PagedStoredEventResult> GetStoredEventsPagedAsync<TId, TValue>(
            TId aggregateId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>;
    }
}