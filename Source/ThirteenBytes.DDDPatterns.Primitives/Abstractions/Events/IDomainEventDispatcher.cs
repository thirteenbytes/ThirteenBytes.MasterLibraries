namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events
{
    public interface IDomainEventDispatcher
    {
        Task DispatchEventAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
    }
}
