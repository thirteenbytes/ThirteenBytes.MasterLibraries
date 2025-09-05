namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Interface for entities that require audit tracking with creation and modification timestamps.
    /// Provides a contract for automatic timestamp management by the persistence layer.
    /// </summary>
    public interface IAuditEntity
    {
        /// <summary>
        /// Gets or sets the UTC timestamp when the entity was created.
        /// Typically set automatically by the persistence layer on first save.
        /// </summary>
        public DateTime CreatedDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the UTC timestamp when the entity was last modified.
        /// Typically updated automatically by the persistence layer on each save.
        /// </summary>
        public DateTime LastModifiedDateUtc { get; set; }
    }
}
