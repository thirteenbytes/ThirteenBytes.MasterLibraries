using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Aggregate Root base implementation with strongly-typed ID and auditing capabilities.
    /// Provides event sourcing support with automatic event management, version tracking,
    /// and in-memory event handler registration for domain events.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier, implementing IEntityId.</typeparam>
    /// <typeparam name="TValue">The underlying type of the identifier value.</typeparam>
    public abstract class AggregateRoot<TId, TValue>
        : AuditEntity<TId, TValue>, IAggregateRoot
        where TId : IEntityId<TId, TValue>
        where TValue : notnull, IEquatable<TValue>
    {
        // Uncommitted domain events
        private readonly List<IDomainEvent> _domainEvents = new();

        // Route table for event handlers
        private readonly Dictionary<Type, Action<IDomainEvent>> _handlers = new();

        /// <summary>
        /// Gets the current version of the aggregate.
        /// Version increments with each applied or replayed event and is used for optimistic concurrency control.
        /// </summary>
        public int Version { get; protected set; }

        /// <summary>
        /// Gets a read-only view of uncommitted domain events.
        /// These events will be persisted and published when the aggregate is saved.
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <summary>
        /// Parameterless constructor for Entity Framework Core materialization and event sourcing reconstruction.
        /// Should not be used directly in domain code. Use this constructor to register event handlers in derived classes.
        /// </summary>
        protected AggregateRoot() { }

        /// <summary>
        /// Domain constructor that creates an aggregate root with the specified identifier.
        /// Typically called with NewId() to create a new aggregate instance.
        /// </summary>
        /// <param name="id">The identifier for the aggregate root. Cannot be null.</param>
        protected AggregateRoot(TId id) : base(id) { }

        /// <summary>
        /// Registers an in-memory event handler for a specific domain event type.
        /// Handlers are used to mutate aggregate state when events are applied or replayed.
        /// </summary>
        /// <typeparam name="TEvent">The type of domain event to handle.</typeparam>
        /// <param name="handler">The handler function that processes the event and mutates state.</param>
        protected void On<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent =>
            _handlers[typeof(TEvent)] = e => handler((TEvent)e);

        /// <summary>
        /// Applies a domain event to the aggregate: routes to registered handler, records as uncommitted, and increments version.
        /// This is the primary method for making state changes in event-sourced aggregates.
        /// </summary>
        /// <param name="event">The domain event to apply to the aggregate.</param>
        /// <returns>A Result indicating success or failure of the event application.</returns>
        protected Result Apply(IDomainEvent @event)
        {
            var result = When(@event);
            if (result.IsFailure) return result;

            _domainEvents.Add(@event);
            Version++;
            return Result.Success();
        }

        /// <summary>
        /// Replays a sequence of historical events to rebuild aggregate state.
        /// Mutates state via registered handlers without recording events as uncommitted.
        /// Used for event sourcing reconstruction from persisted event streams.
        /// </summary>
        /// <param name="history">The sequence of historical events to replay.</param>
        /// <returns>A Result indicating success or failure of the replay operation.</returns>
        public Result Replay(IEnumerable<IDomainEvent> history)
        {
            foreach (var e in history)
            {
                var result = When(e);
                if (result.IsFailure) return result;

                Version++;
            }
            // No Touch() here—replay should mirror historical timestamps; let caller set audit as needed.
            return Result.Success();
        }

        /// <summary>
        /// Routes a domain event to its registered handler for state mutation.
        /// Returns failure if no handler is registered for the event type.
        /// </summary>
        /// <param name="event">The domain event to route to a handler.</param>
        /// <returns>A Result indicating success or failure of the event handling.</returns>
        protected Result When(IDomainEvent @event)
        {
            if (_handlers.TryGetValue(@event.GetType(), out var handler))
            {
                handler(@event);
                return Result.Success();
            }

            return Result.Failure(Error.InternalError(
                $"No handler registered for '{@event.GetType().Name}'."));
        }

        /// <summary>
        /// Clears all uncommitted domain events from the aggregate.
        /// Typically called after events have been successfully persisted and published.
        /// </summary>
        public void ClearDomainEvents() => _domainEvents.Clear();

        /// <summary>
        /// Gets all uncommitted domain events that need to be persisted and published.
        /// These events represent changes that have occurred since the last save operation.
        /// </summary>
        /// <returns>An enumerable of uncommitted domain events.</returns>
        public IEnumerable<IDomainEvent> GetUncommittedEvents() => _domainEvents.AsReadOnly();
    }
}