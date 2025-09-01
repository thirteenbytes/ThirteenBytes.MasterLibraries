namespace ThirteenBytes.DDDPatterns.Primitives.Data
{
    public sealed class PagedStoredEventResult
    {
        public IEnumerable<StoredEvent> Events { get; init; } = Enumerable.Empty<StoredEvent>();
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}