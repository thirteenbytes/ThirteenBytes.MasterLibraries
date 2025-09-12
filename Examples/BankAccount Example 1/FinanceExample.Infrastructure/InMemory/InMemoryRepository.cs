using System.Collections.Concurrent;
using System.Linq.Expressions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;

namespace FinanceExample.Infrastructure.InMemory
{
    internal sealed class InMemoryRepository<T, TId>(InMemoryDatabase database)
        : IRepository<T, TId>
        where T : class, IEntity<TId>
        where TId : notnull
    {
        private readonly ConcurrentDictionary<object, T> _store = database.GetStore<T, TId>();

        public Task<T?> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var compiled = predicate.Compile();
            return Task.FromResult(_store.Values.FirstOrDefault(compiled));
        }

        public Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(id, out var entity);
            return Task.FromResult(entity);
        }

        public Task<List<T>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_store.Values.ToList());

        public Task<bool> AnyAsync(TId id, CancellationToken cancellationToken = default)
            => Task.FromResult(_store.ContainsKey(id));

        public Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            // stage add; SaveChangesAsync will stamp audit + TryAdd
            database.TrackAdd<T>(entity.Id, entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            // stage update; SaveChangesAsync will stamp LastModified + upsert
            database.TrackUpdate<T>(entity.Id, entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            // stage remove; SaveChangesAsync will TryRemove
            database.TrackRemove<T>(entity.Id);
            return Task.CompletedTask;
        }
    }
}
