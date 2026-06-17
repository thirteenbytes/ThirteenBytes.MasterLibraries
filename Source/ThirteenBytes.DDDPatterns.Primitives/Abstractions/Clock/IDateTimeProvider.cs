namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions.Clock
{
    /// <summary>
    /// Provides access to current date and time values in a testable way.
    /// This abstraction allows for time-based logic to be unit tested by providing
    /// controllable date/time values instead of relying on system clock directly.
    /// </summary>
    public interface IDateTimeProvider
    {
        /// <summary>
        /// Gets the current UTC date and time.
        /// In production implementations, this typically returns DateTime.UtcNow.
        /// In test implementations, this can return fixed or controllable values.
        /// </summary>
        DateTime UtcNow { get; }
        
        /// <summary>
        /// Gets the current UTC instant as a <see cref="DateTimeOffset"/> with a zero offset.
        /// Defaults to <see cref="UtcNow"/> projected onto UTC, so existing implementors
        /// (and test doubles) need only supply <see cref="UtcNow"/>.
        /// </summary>
        DateTimeOffset UtcNowOffset { get; } 

    }
}
