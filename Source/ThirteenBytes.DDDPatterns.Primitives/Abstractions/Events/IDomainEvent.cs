namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events
{
    public interface IDomainEvent
    {
        Guid Id { get; }
    }
}
