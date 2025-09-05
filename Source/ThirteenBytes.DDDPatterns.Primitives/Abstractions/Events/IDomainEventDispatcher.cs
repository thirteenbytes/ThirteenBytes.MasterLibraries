namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events
{
    /// <summary>
    /// Interface for dispatching domain events to their respective handlers.
    /// This abstraction enables decoupled event processing where domain events
    /// can be published and handled asynchronously without tight coupling between
    /// the event producer and event consumers.
    /// </summary>
    public interface IDomainEventDispatcher
    {
        /// <summary>
        /// Asynchronously dispatches a collection of domain events to their registered handlers.
        /// Each event will be routed to its appropriate handler(s) for processing.
        /// This method is typically called after an aggregate's events have been persisted
        /// to ensure reliable event processing.
        /// </summary>
        /// <param name="domainEvents">The collection of domain events to dispatch.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the dispatch operation.</param>
        /// <returns>A task representing the asynchronous dispatch operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when domainEvents is null.</exception>
        Task DispatchEventAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
    }
}
