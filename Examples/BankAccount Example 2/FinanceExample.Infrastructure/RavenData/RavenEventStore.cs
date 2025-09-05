using FinanceExample.Infrastructure.RavenData.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Clock;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;
using ThirteenBytes.DDDPatterns.Primitives.Data;

namespace FinanceExample.Infrastructure.RavenData
{
    internal sealed class RavenEventStore : IEventStore
    {
        private readonly IDocumentStore _documentStore;
        private readonly IDateTimeProvider _clock;

        public RavenEventStore(IDocumentStore documentStore, IDateTimeProvider clock)
        {
            _documentStore = documentStore;
            _clock = clock;
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

            var eventsList = events.ToList();
            if (!eventsList.Any())
                return;

            using var session = _documentStore.OpenAsyncSession();

            var streamId = RavenEventStream.CreateId(typeof(TId).Name, aggregateId.Value.ToString()!);
            var stream = await session.LoadAsync<RavenEventStream>(streamId, cancellationToken);

            if (stream == null)
            {
                stream = new RavenEventStream
                {
                    Id = streamId,
                    AggregateId = aggregateId.Value.ToString()!,
                    AggregateType = typeof(TId).Name,
                    Version = 0,
                    Events = new List<RavenStoredEvent>()
                    };
            }

            // Optimistic concurrency check
            if (stream.Version != expectedVersion)
            {
                throw new InvalidOperationException(
                    $"Concurrency conflict: Expected version {expectedVersion}, but current version is {stream.Version}");
            }

            var timestamp = _clock.UtcNow;
            foreach (var domainEvent in eventsList)
            {
                stream.Version++;
                stream.Events.Add(new RavenStoredEvent
                {
                    EventId = domainEvent.Id,
                    EventType = domainEvent.GetType().Name,
                    EventData = domainEvent,
                    Timestamp = timestamp,
                    Version = stream.Version
                });
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
            return await GetEventsAsync<TId, TValue>(aggregateId, 0, cancellationToken);
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
                return Enumerable.Empty<IDomainEvent>();

            return stream.Events
                .Where(e => e.Version > fromVersion)
                .OrderBy(e => e.Version)
                .Select(e => e.EventData)
                .ToList();
        }

        public async Task<PagedEventResult> GetEventsPagedAsync<TId, TValue>(
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
                return new PagedEventResult
                {
                    Events = Enumerable.Empty<IDomainEvent>(),
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

            return new PagedEventResult
            {
                Events = events,
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

        public async Task<PagedStoredEventResult> GetStoredEventsPagedAsync<TId, TValue>(
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
                return new PagedStoredEventResult
                {
                    Events = Enumerable.Empty<StoredEvent>(),
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
                .Select(e => new StoredEvent
                {
                    EventId = e.EventId,
                    EventType = e.EventType,
                    EventData = e.EventData,
                    Timestamp = e.Timestamp,
                    Version = e.Version
                })
                .ToList();

            return new PagedStoredEventResult
            {
                Events = events,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}