using FinanceExample.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceExample.Infrastructure.Core
{
    internal sealed class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public async Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
        {
            var requestType = request.GetType();

            // Resolve IRequestHandler<TRequest, TResult>
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResult));
            var handler = _serviceProvider.GetRequiredService(handlerType);

            // Build the behavior chain: behaviors wrap handler in reverse order
            var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResult));
            var behaviors = _serviceProvider.GetServices(behaviorType).Reverse().ToArray();

            // Terminal delegate: invoke the actual handler
            Func<Task<TResult>> HandlerInvoke = () =>
            {
                var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResult>, TResult>.Handle))!;
                return (Task<TResult>)method.Invoke(handler, new object[] { request, cancellationToken })!;
            };

            // Fold behaviors
            var next = HandlerInvoke;
            foreach (var behavior in behaviors)
            {
                var currentBehavior = behavior;
                var currentNext = next;
                var method = behaviorType.GetMethod(nameof(IPipelineBehavior<IRequest<TResult>, TResult>.Handle))!;
                
                next = () => 
                    (Task<TResult>)method.Invoke(currentBehavior, new object[] { request, cancellationToken, currentNext })!;
            }

            return await next();
        }
    }
}