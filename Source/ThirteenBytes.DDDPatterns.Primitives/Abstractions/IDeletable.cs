namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Interface for entities that support soft delete (logical deletion).
    /// </summary>
    public interface IDeletable
    {
        /// <summary>
        /// Gets the UTC timestamp when the entity was soft deleted.
        /// Null indicates active, non-null indicates deleted.
        /// </summary>
        DateTime? DeletedAtUtc { get; }

        /// <summary>
        /// Marks the entity as soft deleted with the specified timestamp.
        /// </summary>
        /// <param name="deletedAtUtc">The UTC timestamp when deletion occurred.</param>
        void MarkAsDeleted(DateTime deletedAtUtc);

        /// <summary>
        /// Restores a soft deleted entity back to active state (undo/compensation).
        /// </summary>
        void Restore();
    }
}
