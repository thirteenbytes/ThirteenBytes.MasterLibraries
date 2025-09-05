using System.Linq.Expressions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;

namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data
{
    /// <summary>
    /// Generic repository interface for aggregate root persistence operations.
    /// Provides standard CRUD operations with strongly-typed identifiers and async support.
    /// Follows the Repository pattern from Domain-Driven Design.
    /// </summary>
    /// <typeparam name="T">The type of the aggregate root entity.</typeparam>
    /// <typeparam name="TId">The type of the entity's identifier.</typeparam>
    /// <typeparam name="TValue">The underlying type of the identifier value.</typeparam>
    public interface IRepository<T, TId, TValue>
        where T : IEntity<TId, TValue>
        where TId : IEntityId<TId, TValue>
        where TValue : notnull, IEquatable<TValue>
    {
        /// <summary>
        /// Retrieves a single entity that matches the specified predicate.
        /// Returns null if no matching entity is found.
        /// </summary>
        /// <param name="predicate">A function to test each entity for a condition.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task containing the first entity that matches the predicate, or null if none found.</returns>
        Task<T?> GetAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves an entity by its unique identifier.
        /// Returns null if no entity with the specified ID is found.
        /// </summary>
        /// <param name="id">The unique identifier of the entity to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task containing the entity with the specified ID, or null if not found.</returns>
        Task<T?> GetByIdAsync(
            TId id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all entities of the specified type.
        /// Use with caution in production environments with large datasets.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task containing a list of all entities.</returns>
        Task<List<T>> ListAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether an entity with the specified identifier exists.
        /// More efficient than retrieving the full entity when only existence needs to be verified.
        /// </summary>
        /// <param name="id">The unique identifier to check for existence.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task containing true if an entity with the specified ID exists; otherwise, false.</returns>
        Task<bool> AnyAsync(
            TId id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new entity to the repository.
        /// The entity should have a unique identifier and should not already exist.
        /// </summary>
        /// <param name="entity">The entity to add to the repository.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous add operation.</returns>
        Task AddAsync(
            T entity,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing entity in the repository.
        /// The entity should already exist and have the same identifier.
        /// </summary>
        /// <param name="entity">The entity with updated values.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous update operation.</returns>
        Task UpdateAsync(
            T entity,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an entity from the repository.
        /// The entity should exist in the repository before deletion.
        /// </summary>
        /// <param name="entity">The entity to remove from the repository.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        Task DeleteAsync(
            T entity,
            CancellationToken cancellationToken = default);
    }
}
