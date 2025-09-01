using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;

namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    public interface IAggregateRoot
    {
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
        void ClearDomainEvents();                
        IEnumerable<IDomainEvent> GetUncommittedEvents();
        int Version { get; }
    }
}
