using System.Collections.Concurrent;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Clock;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;

namespace FinanceExample.Infrastructure.InMemory
{
    internal sealed class InMemoryDatabase : IUnitOfWork
    {
        private static readonly ConcurrentDictionary<Type, object> _stores = new();
        private readonly List<TrackedOperation> _operations = new();
        private readonly IDateTimeProvider _clock;

        public InMemoryDatabase(IDateTimeProvider clock) => _clock = clock;

        public ConcurrentDictionary<TValue, T> GetStore<T, TId, TValue>()
            where T : class
            where TValue : notnull
        {
            return (ConcurrentDictionary<TValue, T>)_stores.GetOrAdd(
                typeof(T),
                _ => new ConcurrentDictionary<TValue, T>());
        }

        // repos call these instead of mutating the dictionary directly
        public void TrackAdd<T, TValue>(TValue key, T entity)
            where T : class where TValue : notnull =>
            _operations.Add(TrackedOperation.Add(typeof(T), key!, entity!));

        public void TrackUpdate<T, TValue>(TValue key, T entity)
            where T : class where TValue : notnull =>
            _operations.Add(TrackedOperation.Update(typeof(T), key!, entity!));

        public void TrackRemove<T, TValue>(TValue key)
            where T : class where TValue : notnull =>
            _operations.Add(TrackedOperation.Remove(typeof(T), key!));

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = _clock.UtcNow;

            foreach (var operation in _operations)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var storeObject = _stores[operation.EntityType];
                dynamic store = storeObject; // ConcurrentDictionary<TKey, T>

                switch (operation.Kind)
                {
                    case TrackedType.Add:
                        StampCreatedAndModified(operation.Entity!, now);
                        store.TryAdd((dynamic)operation.Key!, (dynamic)operation.Entity!);
                        break;

                    case TrackedType.Update:
                        StampModified(operation.Entity!, now);
                        store[(dynamic)operation.Key!] = (dynamic)operation.Entity!;
                        break;

                    case TrackedType.Remove:
                        dynamic removed;
                        store.TryRemove((dynamic)operation.Key!, out removed);
                        break;
                }
            }

            var count = _operations.Count;
            _operations.Clear();
            return Task.FromResult(count);
        }

        private static void StampCreatedAndModified(object entity, DateTime now)
        {
            if (entity is IAuditEntity a)
            {
                a.CreatedDateUtc = now;
                a.LastModifiedDateUtc = now;
            }
        }
        
        private static void StampModified(object entity, DateTime now)
        {
            if (entity is IAuditEntity a)
                a.LastModifiedDateUtc = now;
        }

        private sealed record TrackedOperation(TrackedType Kind, Type EntityType, object Key, object? Entity)
        {
            public static TrackedOperation Add(Type t, object key, object entity) => new(TrackedType.Add, t, key, entity);
            public static TrackedOperation Update(Type t, object key, object entity) => new(TrackedType.Update, t, key, entity);
            public static TrackedOperation Remove(Type t, object key) => new(TrackedType.Remove, t, key, null);
        }

        private enum TrackedType { Add, Update, Remove }
    }
}
