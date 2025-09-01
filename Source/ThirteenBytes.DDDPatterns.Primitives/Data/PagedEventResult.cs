using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;

namespace ThirteenBytes.DDDPatterns.Primitives.Data
{
    public sealed class PagedEventResult
    {
        public IEnumerable<IDomainEvent> Events { get; init; } = Enumerable.Empty<IDomainEvent>();
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}