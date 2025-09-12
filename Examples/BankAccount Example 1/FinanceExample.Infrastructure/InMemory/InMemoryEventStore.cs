using FinanceExample.Infrastructure.InMemory.Models;
using System.Collections.Concurrent;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Clock;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;
using ThirteenBytes.DDDPatterns.Primitives.Data;

namespace FinanceExample.Infrastructure.InMemory
{

    internal sealed class InMemoryEventStore : IEventStore
    {
        private static readonly ConcurrentDictionary<string, InMemoryEventStream> _eventStreams = new();
        private readonly IDateTimeProvider _clock;

        public InMemoryEventStore(IDateTimeProvider clock)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        }

        public Task AppendEventsAsync<TId>(
            TId aggregateId,
            IEnumerable<IDomainEvent> events,
            int expectedVersion,
            CancellationToken cancellationToken = default)
            where TId : notnull
        {
            if (aggregateId == null)
                throw new ArgumentNullException(nameof(aggregateId));

            var streamKey = GetStreamKey<TId>(aggregateId);
            var eventsList = events.ToList();

            if (!eventsList.Any())
                return Task.CompletedTask;

            var stream = _eventStreams.GetOrAdd(streamKey, _ => new InMemoryEventStream
            {
                AggregateId = aggregateId.ToString()!,
                AggregateType = typeof(TId).Name,
                Version = 0
            });

            lock (stream)
            {
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
                    stream.Events.Add(new InMemoryStoredEvent
                    {
                        EventId = domainEvent.Id,
                        EventType = domainEvent.GetType().Name,
                        EventData = domainEvent,
                        Timestamp = timestamp,
                        Version = stream.Version
                    });
                }
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<IDomainEvent>> GetEventsAsync<TId>(
            TId aggregateId,
            CancellationToken cancellationToken = default)
            where TId : notnull
        {
            return GetEventsAsync<TId>(aggregateId, 0, cancellationToken);
        }

        public Task<IEnumerable<IDomainEvent>> GetEventsAsync<TId>(
            TId aggregateId,
            int fromVersion,
            CancellationToken cancellationToken = default)
            where TId : notnull
        {
            if (aggregateId == null)
                throw new ArgumentNullException(nameof(aggregateId));

            var streamKey = GetStreamKey<TId>(aggregateId);

            if (!_eventStreams.TryGetValue(streamKey, out var stream))
            {
                return Task.FromResult(Enumerable.Empty<IDomainEvent>());
            }

            lock (stream)
            {
                var events = stream.Events
                    .Where(e => e.Version > fromVersion)
                    .OrderBy(e => e.Version)
                    .Select(e => e.EventData)
                    .ToList();

                return Task.FromResult<IEnumerable<IDomainEvent>>(events);
            }
        }

        public Task<PagedResult<IDomainEvent>> GetEventsPagedAsync<TId>(
            TId aggregateId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
            where TId : notnull
        {
            if (aggregateId == null)
                throw new ArgumentNullException(nameof(aggregateId));

            if (pageNumber < 1)
                throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            var streamKey = GetStreamKey<TId>(aggregateId);

            if (!_eventStreams.TryGetValue(streamKey, out var stream))
            {
                return Task.FromResult(new PagedResult<IDomainEvent>
                {
                    Items = Enumerable.Empty<IDomainEvent>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
            }

            lock (stream)
            {
                var totalCount = stream.Events.Count;
                var skip = (pageNumber - 1) * pageSize;

                var events = stream.Events
                    .OrderBy(e => e.Version)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(e => e.EventData)
                    .ToList();

                return Task.FromResult(new PagedResult<IDomainEvent>
                {
                    Items = events,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                });
            }
        }

        public Task<int> GetAggregateVersionAsync<TId>(
            TId aggregateId,
            CancellationToken cancellationToken = default)
            where TId : notnull
        {
            if (aggregateId == null)
                throw new ArgumentNullException(nameof(aggregateId));

            var streamKey = GetStreamKey<TId>(aggregateId);

            if (!_eventStreams.TryGetValue(streamKey, out var stream))
            {
                return Task.FromResult(0);
            }

            lock (stream)
            {
                return Task.FromResult(stream.Version);
            }
        }

        public Task<bool> AggregateExistsAsync<TId>(
            TId aggregateId,
            CancellationToken cancellationToken = default)
            where TId : notnull
        {
            if (aggregateId == null)
                throw new ArgumentNullException(nameof(aggregateId));

            var streamKey = GetStreamKey<TId>(aggregateId);
            var exists = _eventStreams.ContainsKey(streamKey);

            return Task.FromResult(exists);
        }

        private static string GetStreamKey<TId>(TId aggregateId)
            where TId : notnull
        {
            return $"{typeof(TId).Name}_{aggregateId}";
        }
    }
}