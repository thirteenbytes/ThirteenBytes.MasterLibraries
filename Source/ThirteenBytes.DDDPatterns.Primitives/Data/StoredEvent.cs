using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;

namespace ThirteenBytes.DDDPatterns.Primitives.Data
{
    /// <summary>
    /// Represents a domain event that has been persisted to the event store with additional metadata.
    /// Contains the original domain event data along with storage-specific information like timestamps,
    /// version numbers, and type information needed for event sourcing and audit trails.
    /// </summary>
    public sealed class StoredEvent
    {
        /// <summary>
        /// Gets or sets the unique identifier of the stored event.
        /// This may be different from the domain event's ID and is used for event store operations.
        /// </summary>
        public Guid EventId { get; init; }

        /// <summary>
        /// Gets or sets the fully qualified type name of the domain event.
        /// Used for event deserialization and type resolution when loading events from storage.
        /// </summary>
        public string EventType { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the original domain event data.
        /// Contains the business logic and state changes captured by the event.
        /// </summary>
        public IDomainEvent EventData { get; init; } = null!;

        /// <summary>
        /// Gets or sets the timestamp when the event was stored in the event store.
        /// Provides an audit trail and ordering information for event processing.
        /// </summary>
        public DateTime Timestamp { get; init; }

        /// <summary>
        /// Gets or sets the version number of the aggregate when this event was applied.
        /// Used for event ordering within an aggregate's event stream and optimistic concurrency control.
        /// </summary>
        public int Version { get; init; }
    }
}