using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Aggregate Root base with strongly-typed ID and auditing.
    /// </summary>
    public abstract class AggregateRoot<TId, TValue>
        : AuditEntity<TId, TValue>, IAggregateRoot
        where TId : IEntityId<TId, TValue>
        where TValue : notnull, IEquatable<TValue>
    {
        // Uncommitted domain events
        private readonly List<IDomainEvent> _domainEvents = new();

        // Route table for event handlers
        private readonly Dictionary<Type, Action<IDomainEvent>> _handlers = new();

        /// <summary>Version increments with each applied/replayed event.</summary>
        public int Version { get; protected set; }

        /// <summary>Read-only view of uncommitted events.</summary>
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        // EF Core materialization
        protected AggregateRoot() { }

        // Domain construction (typically pass NewId())
        protected AggregateRoot(TId id) : base(id) { }

        /// <summary>Register an in-memory event handler for TEvent.</summary>
        protected void On<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent =>
            _handlers[typeof(TEvent)] = e => handler((TEvent)e);

        /// <summary>
        /// Apply an event: mutates state via registered handler, records it as uncommitted, bumps Version.
        /// </summary>
        protected Result Apply(IDomainEvent @event)
        {
            var result = When(@event);
            if (result.IsFailure) return result;

            _domainEvents.Add(@event);
            Version++;
            return Result.Success();
        }

        /// <summary>
        /// Replay history: mutates state without recording uncommitted events.
        /// </summary>
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
        /// Routes to a registered handler; returns failure if none is found.
        /// </summary>
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

        public void ClearDomainEvents() => _domainEvents.Clear();

        public IEnumerable<IDomainEvent> GetUncommittedEvents() => _domainEvents.AsReadOnly();
    }
}