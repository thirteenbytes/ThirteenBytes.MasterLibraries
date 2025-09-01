using FinanceExample.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;

namespace FinanceExample.Infrastructure.Behaviors
{
    internal sealed class EventStoreBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        private readonly IEventStore _eventStore;
        private readonly ILogger<EventStoreBehavior<TRequest, TResponse>> _logger;

        public EventStoreBehavior(
            IEventStore eventStore,
            ILogger<EventStoreBehavior<TRequest, TResponse>> logger)
        {
            _eventStore = eventStore;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            Func<Task<TResponse>> next)
        {
            // Execute the handler first
            var response = await next();

            // After successful execution, check if we have a Result<T> with an aggregate
            if (IsSuccessfulResult(response) && TryExtractAggregate(response, out var aggregate) && aggregate != null)
            {
                await SaveAggregateEvents(aggregate, cancellationToken);
            }

            return response;
        }

        private static bool IsSuccessfulResult<T>(T response)
        {
            if (response?.GetType().IsGenericType == true && 
                response.GetType().GetGenericTypeDefinition().Name.StartsWith("Result"))
            {
                var isSuccessProperty = response.GetType().GetProperty("IsSuccess");
                return (bool)(isSuccessProperty?.GetValue(response) ?? false);
            }
            return false;
        }

        private static bool TryExtractAggregate<T>(T response, out IAggregateRoot? aggregate)
        {
            aggregate = null;
            
            // Try to extract aggregate from Result<TAggregate> responses
            if (response?.GetType().IsGenericType == true)
            {
                var valueProperty = response.GetType().GetProperty("Value");
                var value = valueProperty?.GetValue(response);
                
                if (value is IAggregateRoot aggregateRoot)
                {
                    aggregate = aggregateRoot;
                    return true;
                }
            }
            
            return false;
        }

        private async Task SaveAggregateEvents(IAggregateRoot aggregate, CancellationToken cancellationToken)
        {
            var events = aggregate.GetUncommittedEvents();
            if (!events.Any()) return;

            try
            {
                // Get the aggregate ID using reflection (could be improved with interfaces)
                var idProperty = aggregate.GetType().GetProperty("Id");
                var aggregateId = idProperty?.GetValue(aggregate);
                
                if (aggregateId != null)
                {
                    var currentVersion = aggregate.Version - events.Count();
                    
                    // Use reflection to call the generic AppendEventsAsync method
                    var method = _eventStore.GetType().GetMethod("AppendEventsAsync");
                    var genericMethod = method?.MakeGenericMethod(
                        aggregateId.GetType(), 
                        aggregateId.GetType().GetGenericArguments()[1]);
                    
                    var task = (Task?)genericMethod?.Invoke(
                        _eventStore, 
                        new object[] { aggregateId, events, currentVersion, cancellationToken });
                    
                    if (task != null)
                    {
                        await task;
                        aggregate.ClearDomainEvents();
                        
                        _logger.LogInformation(
                            "Saved {EventCount} events for aggregate {AggregateType} with ID {AggregateId}",
                            events.Count(),
                            aggregate.GetType().Name,
                            aggregateId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Failed to save events for aggregate {AggregateType}", 
                    aggregate.GetType().Name);
                throw;
            }
        }
    }
}