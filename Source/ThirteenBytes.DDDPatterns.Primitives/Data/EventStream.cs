using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;

namespace ThirteenBytes.DDDPatterns.Primitives.Data
{
    /// <summary>
    /// Represents a complete event stream for a specific aggregate instance.
    /// Contains the aggregate's metadata along with all its stored events,
    /// providing a complete audit trail and state reconstruction capability.
    /// </summary>
    public sealed class EventStream
    {
        /// <summary>
        /// Gets or sets the unique identifier of the aggregate this event stream belongs to.
        /// Typically stored as a string representation of the strongly-typed aggregate ID.
        /// </summary>
        public string AggregateId { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the type name of the aggregate this event stream represents.
        /// Used for event store organization and aggregate reconstruction.
        /// </summary>
        public string AggregateType { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the current version of the aggregate based on the number of events.
        /// Used for optimistic concurrency control and event ordering.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the collection of stored events that make up this event stream.
        /// Events are typically ordered by their version/sequence number to maintain proper ordering.
        /// </summary>
        public List<StoredEvent> Events { get; init; } = new();
    }
}