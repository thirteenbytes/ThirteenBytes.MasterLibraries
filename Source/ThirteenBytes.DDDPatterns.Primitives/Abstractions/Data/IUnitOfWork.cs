namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data
{
    /// <summary>
    /// Represents a unit of work pattern implementation for managing transactions
    /// and coordinating writes across multiple repositories within a single business operation.
    /// Ensures data consistency by committing or rolling back all changes as a single unit.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Asynchronously saves all pending changes within the current unit of work.
        /// All repository changes made during the current operation will be committed
        /// as a single transaction, ensuring data consistency.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the save operation.</param>
        /// <returns>A task containing the number of entities that were affected by the save operation.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
