using System.Linq.Expressions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;

namespace FCT.DDD.Primitives.Abstractions.Data
{
    public interface IRepository<T, TId, TValue>
        where T : IEntity<TId, TValue>
        where TId : IEntityId<TId, TValue>
        where TValue : notnull, IEquatable<TValue>
    {
        Task<T?> GetAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<T?> GetByIdAsync(
            TId id,
            CancellationToken cancellationToken = default);

        Task<List<T>> ListAsync(
            CancellationToken cancellationToken = default);

        Task<bool> AnyAsync(
            TId id,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            T entity,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            T entity,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            T entity,
            CancellationToken cancellationToken = default);
    }
}
