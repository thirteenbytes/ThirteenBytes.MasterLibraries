using Raven.Client.Documents;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;
using ThirteenBytes.DDDPatterns.Primitives.Data;
using FinanceExample.Infrastructure.RavenData.Models;

namespace FinanceExample.Infrastructure.RavenData
{
    internal sealed class RavenEventStore : IEventStore
    {
        private readonly IDocumentStore _documentStore;

        public RavenEventStore(IDocumentStore documentStore)
        {
            _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        }

        public async Task AppendEventsAsync<TId, TValue>(
            TId aggregateId,
            IEnumerable<IDomainEvent> events,
            int expectedVersion,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>
        {
            if (aggregateId == null)
                throw new ArgumentNullException(nameof(aggregateId));

            using var session = _documentStore.OpenAsyncSession();

            var streamId = RavenEventStream.CreateId(typeof(TId).Name, aggregateId.Value.ToString()!);
            var stream = await session.LoadAsync<RavenEventStream>(streamId, cancellationToken);

            if (stream == null)
            {
                if (expectedVersion != 0)
                {
                    throw new InvalidOperationException(
                        $"Expected version {expectedVersion}, but aggregate does not exist");
                }

                stream = new RavenEventStream
                {
                    Id = streamId,
                    AggregateId = aggregateId.Value.ToString()!,
                    AggregateType = typeof(TId).Name,
                    Version = 0
                };
            }
            else if (stream.Version != expectedVersion)
            {
                throw new InvalidOperationException(
                    $"Expected version {expectedVersion}, but current version is {stream.Version}");
            }

            foreach (var domainEvent in events)
            {
                var storedEvent = new RavenStoredEvent
                {
                    EventId = Guid.NewGuid(),
                    EventType = domainEvent.GetType().AssemblyQualifiedName!,
                    EventData = domainEvent,
                    Timestamp = DateTime.UtcNow,
                    Version = ++stream.Version
                };

                stream.Events.Add(storedEvent);
            }

            await session.StoreAsync(stream, cancellationToken);
            await session.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<IDomainEvent>> GetEventsAsync<TId, TValue>(
            TId aggregateId,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>
        {
            if (aggregateId == null)
                throw new ArgumentNullException(nameof(aggregateId));

            using var session = _documentStore.OpenAsyncSession();

            var streamId = RavenEventStream.CreateId(typeof(TId).Name, aggregateId.Value.ToString()!);
            var stream = await session.LoadAsync<RavenEventStream>(streamId, cancellationToken);

            if (stream == null)
            {
                return Enumerable.Empty<IDomainEvent>();
            }

            return stream.Events
                .OrderBy(e => e.Version)
                .Select(e => e.EventData)
                .ToList();
        }

        public async Task<IEnumerable<IDomainEvent>> GetEventsAsync<TId, TValue>(
            TId aggregateId,
            int fromVersion,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>
        {
            if (aggregateId == null)
                throw new ArgumentNullException(nameof(aggregateId));

            using var session = _documentStore.OpenAsyncSession();

            var streamId = RavenEventStream.CreateId(typeof(TId).Name, aggregateId.Value.ToString()!);
            var stream = await session.LoadAsync<RavenEventStream>(streamId, cancellationToken);

            if (stream == null)
            {
                return Enumerable.Empty<IDomainEvent>();
            }

            return stream.Events
                .Where(e => e.Version >= fromVersion)
                .OrderBy(e => e.Version)
                .Select(e => e.EventData)
                .ToList();
        }

        public async Task<PagedResult<IDomainEvent>> GetEventsPagedAsync<TId, TValue>(
            TId aggregateId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>
        {
            if (aggregateId == null)
                throw new ArgumentNullException(nameof(aggregateId));

            if (pageNumber < 1)
                throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            using var session = _documentStore.OpenAsyncSession();

            var streamId = RavenEventStream.CreateId(typeof(TId).Name, aggregateId.Value.ToString()!);
            var stream = await session.LoadAsync<RavenEventStream>(streamId, cancellationToken);

            if (stream == null)
            {
                return new PagedResult<IDomainEvent>
                {
                    Items = Enumerable.Empty<IDomainEvent>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            var totalCount = stream.Events.Count;
            var skip = (pageNumber - 1) * pageSize;

            var events = stream.Events
                .OrderBy(e => e.Version)
                .Skip(skip)
                .Take(pageSize)
                .Select(e => e.EventData)
                .ToList();

            return new PagedResult<IDomainEvent>
            {
                Items = events,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<int> GetAggregateVersionAsync<TId, TValue>(
            TId aggregateId,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>
        {
            if (aggregateId == null)
                throw new ArgumentNullException(nameof(aggregateId));

            using var session = _documentStore.OpenAsyncSession();

            var streamId = RavenEventStream.CreateId(typeof(TId).Name, aggregateId.Value.ToString()!);
            var stream = await session.LoadAsync<RavenEventStream>(streamId, cancellationToken);

            return stream?.Version ?? 0;
        }

        public async Task<bool> AggregateExistsAsync<TId, TValue>(
            TId aggregateId,
            CancellationToken cancellationToken = default)
            where TId : IEntityId<TId, TValue>
            where TValue : notnull, IEquatable<TValue>
        {
            if (aggregateId == null)
                throw new ArgumentNullException(nameof(aggregateId));

            using var session = _documentStore.OpenAsyncSession();

            var streamId = RavenEventStream.CreateId(typeof(TId).Name, aggregateId.Value.ToString()!);
            var exists = await session.Advanced.ExistsAsync(streamId, cancellationToken);

            return exists;
        }
    }
}