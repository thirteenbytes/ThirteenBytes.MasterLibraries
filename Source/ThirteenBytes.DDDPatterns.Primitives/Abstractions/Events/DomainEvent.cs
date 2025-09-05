namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events
{
    /// <summary>
    /// Base record implementation for domain events.
    /// Provides automatic ID generation and immutability through the record type.
    /// Domain events should capture the essential information about what happened,
    /// when it happened, and any relevant context needed by event handlers.
    /// </summary>
    public abstract record DomainEvent : IDomainEvent
    {
        /// <summary>
        /// Gets the unique identifier for this domain event.
        /// Automatically generated when the event is created.
        /// </summary>
        public Guid Id { get; init; } = Guid.NewGuid();
    }
}
