using System;

namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Base implementation for audited entities with strongly-typed identifiers.
    /// Extends Entity to include automatic audit tracking with creation and modification timestamps.
    /// Timestamps are typically managed by the persistence layer (e.g., Entity Framework interceptors).
    /// </summary>
    /// <typeparam name="TId">The type of the entity's identifier, implementing IEntityId.</typeparam>
    /// <typeparam name="TValue">The underlying type of the identifier value.</typeparam>
    public abstract class AuditEntity<TId, TValue> : Entity<TId, TValue>, IAuditEntity
        where TId : IEntityId<TId, TValue>
        where TValue : notnull, IEquatable<TValue>
    {
        /// <summary>
        /// Gets or sets the UTC timestamp when the entity was created.
        /// Automatically set by the persistence layer on first save.
        /// </summary>
        public DateTime CreatedDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp when the entity was last modified.
        /// Automatically updated by the persistence layer on each save.
        /// </summary>
        public DateTime LastModifiedDateUtc { get; set; }

        /// <summary>
        /// Parameterless constructor for Entity Framework Core materialization.
        /// Should not be used directly in domain code.
        /// </summary>
        protected AuditEntity() { }

        /// <summary>
        /// Domain constructor that creates an audited entity with the specified identifier.
        /// Timestamps will be set by the persistence layer when the entity is saved.
        /// </summary>
        /// <param name="id">The identifier for the entity. Cannot be null.</param>
        protected AuditEntity(TId id) : base(id) { }
    }
}