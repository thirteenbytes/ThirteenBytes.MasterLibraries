namespace ThirteenBytes.DDDPatterns.Primitives.Data
{
    /// <summary>
    /// Represents a paginated collection of stored events (with metadata) returned from an event store query.
    /// Similar to PagedEventResult but includes additional event storage metadata like timestamps and versions.
    /// </summary>
    public sealed class PagedStoredEventResult
    {
        /// <summary>
        /// Gets the stored events for the current page.
        /// Contains events with full metadata including timestamps, versions, and storage information.
        /// </summary>
        public IEnumerable<StoredEvent> Events { get; init; } = Enumerable.Empty<StoredEvent>();

        /// <summary>
        /// Gets the total number of stored events across all pages.
        /// Used to calculate pagination metadata and display total counts.
        /// </summary>
        public int TotalCount { get; init; }

        /// <summary>
        /// Gets the current page number (1-based).
        /// Indicates which page of results is currently being returned.
        /// </summary>
        public int PageNumber { get; init; }

        /// <summary>
        /// Gets the number of stored events per page.
        /// Determines how many events are included in each page of results.
        /// </summary>
        public int PageSize { get; init; }

        /// <summary>
        /// Gets the total number of pages available.
        /// Calculated based on the total count and page size.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Gets a value indicating whether there are more pages after the current page.
        /// Useful for implementing "Next" navigation in user interfaces.
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Gets a value indicating whether there are pages before the current page.
        /// Useful for implementing "Previous" navigation in user interfaces.
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
    }
}