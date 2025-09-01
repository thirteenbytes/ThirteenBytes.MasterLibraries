namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    public interface IEntity { }

    // Entity contract tied to your strongly-typed ID
    public interface IEntity<TId, TValue> : IEntity
        where TId : IEntityId<TId, TValue>
        where TValue : notnull, IEquatable<TValue>
    {
        TId Id { get; }
    }
}
