namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{

    /// <summary>
    /// Minimal base that provides value storage + implicit conversion.
    /// (No "New" logic here—that lives in the concrete ID type.)
    /// </summary>
    public abstract record EntityId<TValue>(TValue Value)
        where TValue : notnull, IEquatable<TValue>
    {
        public static implicit operator TValue(EntityId<TValue> id) => id.Value;
        public override string ToString() => Value.ToString()!;
    }
}
