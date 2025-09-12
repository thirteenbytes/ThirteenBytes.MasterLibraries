namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Marker interface for all entities in the domain model.
    /// Provides a common base type for polymorphic operations on entities.
    /// </summary>
    public interface IEntity { }

    /// <summary>
    /// Generic entity contract that requires a strongly-typed identifier.
    /// Entities are distinguished by their identity rather than their attributes,
    /// following Domain-Driven Design principles.
    /// </summary>
    /// <typeparam name="TId">The type of the entity's identifier.</typeparam>
    public interface IEntity<TId> : IEntity
        where TId : notnull
    {
        /// <summary>
        /// Gets the unique identifier for this entity.
        /// Entity equality is determined by comparing identifiers.
        /// </summary>
        TId Id { get; }
    }
}
