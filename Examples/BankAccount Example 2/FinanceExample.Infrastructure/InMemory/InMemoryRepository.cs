using System.Collections.Concurrent;
using System.Linq.Expressions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;

namespace FinanceExample.Infrastructure.InMemory
{
    internal sealed class InMemoryRepository<T, TId, TValue>(InMemoryDatabase database)
        : IRepository<T, TId, TValue>
        where T : class, IEntity<TId, TValue>
        where TId : IEntityId<TId, TValue>
        where TValue : notnull, IEquatable<TValue>
    {
        private readonly ConcurrentDictionary<TValue, T> _store = database.GetStore<T, TId, TValue>();

        public Task<T?> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var compiled = predicate.Compile();
            return Task.FromResult(_store.Values.FirstOrDefault(compiled));
        }

        public Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(id.Value, out var entity);
            return Task.FromResult(entity);
        }

        public Task<List<T>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_store.Values.ToList());

        public Task<bool> AnyAsync(TId id, CancellationToken cancellationToken = default)
            => Task.FromResult(_store.ContainsKey(id.Value));

        public Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            // stage add; SaveChangesAsync will stamp audit + TryAdd
            database.TrackAdd<T, TValue>(entity.Id.Value, entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            // stage update; SaveChangesAsync will stamp LastModified + upsert
            database.TrackUpdate<T, TValue>(entity.Id.Value, entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            // stage remove; SaveChangesAsync will TryRemove
            database.TrackRemove<T, TValue>(entity.Id.Value);
            return Task.CompletedTask;
        }
    }
}
