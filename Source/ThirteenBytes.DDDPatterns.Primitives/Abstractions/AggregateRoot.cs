using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Base implementation for Aggregate Roots in Domain-Driven Design.
    /// Supports a hybrid approach: state + domain events with built-in event sourcing readiness.
    /// All state changes should preferably go through <see cref="Apply"/> so that events drive mutations.
    /// </summary>
    /// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
    public abstract class AggregateRoot<TId> : AuditEntity<TId>, IAggregateRoot
        where TId : notnull
    {
        #region Public Members

        /// <summary>
        /// Gets the current version of the aggregate for optimistic concurrency control.
        /// Incremented with each applied or replayed event.
        /// </summary>
        public int Version { get; protected set; }

        /// <summary>
        /// Gets whether this aggregate has any uncommitted domain events.
        /// </summary>
        public bool HasUncommittedEvents => _domainEvents.Count > 0;

        /// <summary>
        /// Gets a read-only view of uncommitted domain events.
        /// These events will be persisted and published when the aggregate is saved.
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> DomainEvents
            => GetUncommittedEvents();

        /// <summary>
        /// Gets all uncommitted domain events that need to be persisted and published.
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> GetUncommittedEvents()
            => _domainEvents.AsReadOnly();

        /// <summary>
        /// Replays historical events to rebuild aggregate state (used in event sourcing).
        /// Does not record events as uncommitted.
        /// </summary>
        public Result Replay(IEnumerable<IDomainEvent> history)
        {
            foreach (var e in history)
            {
                var result = When(e);
                if (result.IsFailure)
                    return result;

                Version++;
            }
            return Result.Success();
        }

        /// <summary>
        /// Marks all uncommitted events as committed (clears the list).
        /// Should be called by UnitOfWork or EF SaveChangesInterceptor after successful persistence and publishing.
        /// </summary>
        public void MarkChangesAsCommitted() => _domainEvents.Clear();

        /// <summary>
        /// Clears all uncommitted domain events (legacy name for backward compatibility).
        /// Prefer <see cref="MarkChangesAsCommitted"/> in new code.
        /// </summary>
        public void ClearDomainEvents() => MarkChangesAsCommitted();

        #endregion

        #region Protected Members

        /// <summary>
        /// Parameterless constructor for EF Core materialization and event sourcing reconstruction.
        /// Derived classes should register event handlers here via <see cref="On{TEvent}"/>.
        /// </summary>
        protected AggregateRoot() { }

        /// <summary>
        /// Domain constructor for creating a new aggregate with a specific ID.
        /// </summary>
        protected AggregateRoot(TId id) : base(id) { }

        /// <summary>
        /// Registers an in-memory handler that mutates aggregate state when an event is applied or replayed.
        /// </summary>
        protected void On<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent =>
            _handlers[typeof(TEvent)] = e => handler((TEvent)e);

        /// <summary>
        /// Applies a domain event: mutates state via registered handler, records the event as uncommitted,
        /// and increments version. This is the preferred way to make state changes.
        /// </summary>
        protected Result Apply(IDomainEvent @event)
        {
            if (@event == null)
            {
                return Result.Failure(Error.InternalError("Domain event cannot be null."));
            }

            var result = When(@event);
            if (result.IsFailure)
            {
                return result;
            }

            _domainEvents.Add(@event);
            Version++;
            return Result.Success();
        }

        /// <summary>
        /// Raises a domain event without triggering state mutation via handler.
        /// Use this when you mutate state directly and only want to record the fact.
        /// Prefer <see cref="Apply"/> when possible for full event-driven consistency.
        /// </summary>
        protected void RaiseDomainEvent(IDomainEvent @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            _domainEvents.Add(@event);
            Version++;
        }

        /// <summary>
        /// Routes the event to its registered handler for state mutation.
        /// In production, unknown events are ignored (useful during replay of historical events).
        /// </summary>
        protected Result When(IDomainEvent @event)
        {
            if (_handlers.TryGetValue(@event.GetType(), out var handler))
            {
                handler(@event);
                return Result.Success();
            }

#if DEBUG
            return Result.Failure(Error.InternalError(
                $"No handler registered for domain event '{@event.GetType().Name}'."));
#else
            // In production, silently ignore unknown events during replay
            return Result.Success();
#endif
        }

        #endregion

        #region Private Members

        // Uncommitted domain events
        private readonly List<IDomainEvent> _domainEvents = new();

        // Route table for event handlers (state mutation)
        private readonly Dictionary<Type, Action<IDomainEvent>> _handlers = new();

        #endregion
    }
}