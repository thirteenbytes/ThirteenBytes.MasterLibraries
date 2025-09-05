namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events
{
    /// <summary>
    /// Represents a domain event that captures something important that happened in the domain.
    /// Domain events are used to communicate between bounded contexts and trigger side effects.
    /// They should be immutable and contain all information needed to understand what happened.
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Gets the unique identifier for this domain event.
        /// Used for event deduplication, tracking, and correlation.
        /// </summary>
        Guid Id { get; }
    }
}
