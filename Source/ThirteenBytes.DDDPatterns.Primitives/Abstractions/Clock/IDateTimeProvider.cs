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
    }
}
