using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;
using ThirteenBytes.DDDPatterns.Primitives.Data;

namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data
{
    /// <summary>
    /// Interface for event store implementations that provide persistence for domain events.
    /// Supports event sourcing patterns with optimistic concurrency control and event replay capabilities.
    /// </summary>
    public interface IEventStore
    {
        /// <summary>
        /// Appends new events to the event store for a specific aggregate.
        /// Provides optimistic concurrency control by checking the expected version.
        /// </summary>
        /// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
        /// <typeparam name="TValue">The underlying type of the identifier value.</typeparam>
        /// <param name="aggregateId">The unique identifier of the aggregate.</param>
        /// <param name="events">The domain events to append to the event stream.</param>
        /// <param name="expectedVersion">The expected current version of the aggregate for concurrency control.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous append operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the expected version doesn't match the actual version.</exception>
        Task AppendEventsAsync<TId, TValue>(
            TId aggregateId, 
            IEnumerable<IDomainEvent> events, 
            int expectedVersion,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>;

        /// <summary>
        /// Retrieves all events for a specific aggregate from the beginning of its event stream.
        /// </summary>
        /// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
        /// <typeparam name="TValue">The underlying type of the identifier value.</typeparam>
        /// <param name="aggregateId">The unique identifier of the aggregate.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task containing an enumerable of all domain events for the aggregate.</returns>
        Task<IEnumerable<IDomainEvent>> GetEventsAsync<TId, TValue>(
            TId aggregateId,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>;

        /// <summary>
        /// Retrieves events for a specific aggregate starting from a specific version.
        /// Useful for loading aggregate state changes since a known point in time.
        /// </summary>
        /// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
        /// <typeparam name="TValue">The underlying type of the identifier value.</typeparam>
        /// <param name="aggregateId">The unique identifier of the aggregate.</param>
        /// <param name="fromVersion">The version number to start retrieving events from (inclusive).</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task containing an enumerable of domain events starting from the specified version.</returns>
        Task<IEnumerable<IDomainEvent>> GetEventsAsync<TId, TValue>(
            TId aggregateId,
            int fromVersion,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>;

        /// <summary>
        /// Retrieves a paginated list of events for a specific aggregate.
        /// Useful for loading large event streams in smaller chunks.
        /// </summary>
        /// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
        /// <typeparam name="TValue">The underlying type of the identifier value.</typeparam>
        /// <param name="aggregateId">The unique identifier of the aggregate.</param>
        /// <param name="pageNumber">The page number to retrieve (1-based).</param>
        /// <param name="pageSize">The number of events per page.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task containing a paginated result of domain events.</returns>
        Task<PagedEventResult> GetEventsPagedAsync<TId, TValue>(
            TId aggregateId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>;

        /// <summary>
        /// Gets the current version number of an aggregate based on its event count.
        /// Returns 0 if the aggregate doesn't exist.
        /// </summary>
        /// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
        /// <typeparam name="TValue">The underlying type of the identifier value.</typeparam>
        /// <param name="aggregateId">The unique identifier of the aggregate.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task containing the current version number of the aggregate.</returns>
        Task<int> GetAggregateVersionAsync<TId, TValue>(
            TId aggregateId,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>;

        /// <summary>
        /// Checks whether an aggregate exists in the event store.
        /// An aggregate exists if it has at least one persisted event.
        /// </summary>
        /// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
        /// <typeparam name="TValue">The underlying type of the identifier value.</typeparam>
        /// <param name="aggregateId">The unique identifier of the aggregate.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task containing true if the aggregate exists; otherwise, false.</returns>
        Task<bool> AggregateExistsAsync<TId, TValue>(
            TId aggregateId,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>;

        /// <summary>
        /// Retrieves a paginated list of stored events (with metadata) for a specific aggregate.
        /// Unlike GetEventsPagedAsync, this includes event metadata such as timestamps and version numbers.
        /// </summary>
        /// <typeparam name="TId">The type of the aggregate identifier.</typeparam>
        /// <typeparam name="TValue">The underlying type of the identifier value.</typeparam>
        /// <param name="aggregateId">The unique identifier of the aggregate.</param>
        /// <param name="pageNumber">The page number to retrieve (1-based).</param>
        /// <param name="pageSize">The number of events per page.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task containing a paginated result of stored events with metadata.</returns>
        Task<PagedStoredEventResult> GetStoredEventsPagedAsync<TId, TValue>(
            TId aggregateId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>;
    }
}