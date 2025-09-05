namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Contract for strongly-typed entity identifiers that expose static factory methods.
    /// This interface enables type-safe ID creation and conversion patterns in DDD implementations.
    /// Requires C# 11+ (.NET 7+) for static abstract members.
    /// </summary>
    /// <typeparam name="TSelf">The concrete type implementing this interface (self-referencing generic pattern).</typeparam>
    /// <typeparam name="TValue">The underlying type of the identifier value (e.g., Guid, string, int, Ulid).</typeparam>
    public interface IEntityId<TSelf, TValue>
        where TSelf : IEntityId<TSelf, TValue>
        where TValue : notnull, IEquatable<TValue>
    {
        /// <summary>
        /// Creates a new identifier instance with a system-generated value.
        /// Implementation should generate appropriate new values (e.g., Guid.NewGuid(), Ulid.NewUlid()).
        /// </summary>
        /// <returns>A new identifier instance with a unique value.</returns>
        static abstract TSelf New();

        /// <summary>
        /// Creates an identifier instance from an existing value.
        /// Used for reconstruction from persistence, API inputs, or other scenarios where the value is known.
        /// </summary>
        /// <param name="value">The existing value to wrap in the identifier type.</param>
        /// <returns>An identifier instance wrapping the provided value.</returns>
        static abstract TSelf From(TValue value);

        /// <summary>
        /// Gets the underlying value of the identifier.
        /// </summary>
        TValue Value { get; }
    }
}
