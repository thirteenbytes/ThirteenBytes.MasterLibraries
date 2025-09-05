namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Abstract base record for entity identifiers that provides value storage and implicit conversion.
    /// This minimal base class handles the common functionality of wrapping a value and converting back to the underlying type.
    /// Concrete entity ID types should inherit from this and implement IEntityId&lt;TSelf, TValue&gt; for factory methods.
    /// </summary>
    /// <typeparam name="TValue">The underlying type of the identifier value (e.g., Guid, string, int, Ulid).</typeparam>
    /// <param name="Value">The underlying value of the identifier.</param>
    public abstract record EntityId<TValue>(TValue Value)
        where TValue : notnull, IEquatable<TValue>
    {
        /// <summary>
        /// Implicitly converts an EntityId to its underlying value type.
        /// This allows seamless use of the identifier where the underlying value type is expected.
        /// </summary>
        /// <param name="id">The entity identifier to convert.</param>
        /// <returns>The underlying value of the identifier.</returns>
        public static implicit operator TValue(EntityId<TValue> id) => id.Value;

        /// <summary>
        /// Returns a string representation of the underlying identifier value.
        /// </summary>
        /// <returns>A string representation of the identifier value.</returns>
        public override string ToString() => Value.ToString()!;
    }
}
