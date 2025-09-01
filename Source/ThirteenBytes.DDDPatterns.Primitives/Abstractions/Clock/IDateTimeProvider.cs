namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions.Clock
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
    }
}
