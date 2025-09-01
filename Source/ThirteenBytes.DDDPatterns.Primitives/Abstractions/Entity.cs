namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Base entity with strongly-typed ID (supports Guid/string/Ulid/etc.).
    /// Equality is by Id.
    /// </summary>
    public abstract class Entity<TId, TValue> : IEntity<TId, TValue>, IEquatable<Entity<TId, TValue>>
        where TId : IEntityId<TId, TValue>
        where TValue : notnull, IEquatable<TValue>
    {
        // EF Core-friendly: init-only, set via ctor or EF materialization
        public TId Id { get; private init; } = default!;

        protected Entity() { }                           // EF Core
        protected Entity(TId id) => Id = id ?? throw new ArgumentNullException(nameof(id));

        // Convenience for derived types: create a new ID using static abstract New()
        protected static TId NewId() => TId.New();

        public override bool Equals(object? obj) =>
            ReferenceEquals(this, obj) ||
            (obj is Entity<TId, TValue> other && EqualityComparer<TId>.Default.Equals(Id, other.Id));

        public bool Equals(Entity<TId, TValue>? other) =>
             other is not null &&
             (ReferenceEquals(this, other) || EqualityComparer<TId>.Default.Equals(Id, other.Id));

        public override int GetHashCode() =>
            EqualityComparer<TId>.Default.GetHashCode(Id);

        public static bool operator ==(Entity<TId, TValue>? a, Entity<TId, TValue>? b) =>
            a is null ? b is null : a.Equals(b);

        public static bool operator !=(Entity<TId, TValue>? a, Entity<TId, TValue>? b) => !(a == b);
    }
}
