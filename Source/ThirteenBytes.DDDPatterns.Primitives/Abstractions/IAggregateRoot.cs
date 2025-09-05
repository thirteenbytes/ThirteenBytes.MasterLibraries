using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;

namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Interface for aggregate roots in Domain-Driven Design.
    /// Aggregate roots are the only entities that can be directly accessed from outside the aggregate.
    /// They manage domain events and maintain the aggregate's version for optimistic concurrency control.
    /// </summary>
    public interface IAggregateRoot
    {
        /// <summary>
        /// Gets a read-only collection of uncommitted domain events produced by this aggregate.
        /// These events will be published after the aggregate is successfully persisted.
        /// </summary>
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

        /// <summary>
        /// Clears all uncommitted domain events from the aggregate.
        /// Typically called after events have been successfully published.
        /// </summary>
        void ClearDomainEvents();

        /// <summary>
        /// Gets all uncommitted domain events that need to be published.
        /// These events represent changes that have occurred since the last save.
        /// </summary>
        /// <returns>An enumerable of uncommitted domain events.</returns>
        IEnumerable<IDomainEvent> GetUncommittedEvents();

        /// <summary>
        /// Gets the current version of the aggregate.
        /// Used for optimistic concurrency control in event sourcing scenarios.
        /// The version is incremented each time an event is applied or replayed.
        /// </summary>
        int Version { get; }
    }
}
