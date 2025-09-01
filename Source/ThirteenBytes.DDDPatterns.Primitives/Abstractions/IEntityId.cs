namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Contract for strongly-typed IDs that expose static factories.
    /// Requires C# 11+ (.NET 7+) for static abstract members.
    /// </summary>
    public interface IEntityId<TSelf, TValue>
        where TSelf : IEntityId<TSelf, TValue>
        where TValue : notnull, IEquatable<TValue>
    {
        static abstract TSelf New();
        static abstract TSelf From(TValue value);

        TValue Value { get; }
    }
}
