namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Base implementation for entities with strongly-typed identifiers.
    /// Provides identity-based equality semantics and supports various ID types (Guid, string, Ulid, etc.).
    /// Entities are equal if they have the same ID, regardless of other property values.
    /// </summary>
    /// <typeparam name="TId">The type of the entity's identifier, implementing IEntityId.</typeparam>
    /// <typeparam name="TValue">The underlying type of the identifier value.</typeparam>
    public abstract class Entity<TId, TValue> : IEntity<TId, TValue>, IEquatable<Entity<TId, TValue>>
        where TId : IEntityId<TId, TValue>
        where TValue : notnull, IEquatable<TValue>
    {
        /// <summary>
        /// Gets the unique identifier for this entity.
        /// EF Core-friendly: init-only property that can be set via constructor or EF materialization.
        /// </summary>
        public TId Id { get; private init; } = default!;

        /// <summary>
        /// Parameterless constructor for Entity Framework Core materialization.
        /// Should not be used directly in domain code.
        /// </summary>
        protected Entity() { }

        /// <summary>
        /// Domain constructor that requires an identifier.
        /// </summary>
        /// <param name="id">The identifier for the entity. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when id is null.</exception>
        protected Entity(TId id) => Id = id ?? throw new ArgumentNullException(nameof(id));

        /// <summary>
        /// Convenience method for derived types to create a new identifier using the static abstract New() method.
        /// </summary>
        /// <returns>A new identifier instance.</returns>
        protected static TId NewId() => TId.New();

        /// <summary>
        /// Determines whether the specified object is equal to the current entity.
        /// Entities are equal if they have the same ID.
        /// </summary>
        /// <param name="obj">The object to compare with the current entity.</param>
        /// <returns>true if the specified object is equal to the current entity; otherwise, false.</returns>
        public override bool Equals(object? obj) =>
            ReferenceEquals(this, obj) ||
            (obj is Entity<TId, TValue> other && EqualityComparer<TId>.Default.Equals(Id, other.Id));

        /// <summary>
        /// Determines whether the specified entity is equal to the current entity.
        /// Entities are equal if they have the same ID.
        /// </summary>
        /// <param name="other">The entity to compare with the current entity.</param>
        /// <returns>true if the specified entity is equal to the current entity; otherwise, false.</returns>
        public bool Equals(Entity<TId, TValue>? other) =>
             other is not null &&
             (ReferenceEquals(this, other) || EqualityComparer<TId>.Default.Equals(Id, other.Id));

        /// <summary>
        /// Returns the hash code for this entity based on its ID.
        /// </summary>
        /// <returns>A hash code for the current entity.</returns>
        public override int GetHashCode() =>
            EqualityComparer<TId>.Default.GetHashCode(Id);

        /// <summary>
        /// Determines whether two entity instances are equal.
        /// </summary>
        /// <param name="a">The first entity to compare.</param>
        /// <param name="b">The second entity to compare.</param>
        /// <returns>true if the entities are equal; otherwise, false.</returns>
        public static bool operator ==(Entity<TId, TValue>? a, Entity<TId, TValue>? b) =>
            a is null ? b is null : a.Equals(b);

        /// <summary>
        /// Determines whether two entity instances are not equal.
        /// </summary>
        /// <param name="a">The first entity to compare.</param>
        /// <param name="b">The second entity to compare.</param>
        /// <returns>true if the entities are not equal; otherwise, false.</returns>
        public static bool operator !=(Entity<TId, TValue>? a, Entity<TId, TValue>? b) => !(a == b);
    }
}
