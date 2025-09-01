namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events
{
    public abstract record DomainEvent : IDomainEvent
    {
        public Guid Id { get; init; } = Guid.NewGuid();
    }
}
