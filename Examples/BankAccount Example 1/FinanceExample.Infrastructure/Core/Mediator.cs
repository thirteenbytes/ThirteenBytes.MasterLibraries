using FinanceExample.Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceExample.Infrastructure.Core
{
    internal sealed class Mediator : IMediator
    {
        private readonly IServiceProvider _sp;

        public Mediator(IServiceProvider sp) => _sp = sp;

        public async Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken ct = default)
        {
            var requestType = request.GetType();

            // Resolve IRequestHandler<TRequest, TResult>
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResult));
            var handler = _sp.GetRequiredService(handlerType);

            // Build the behavior chain: behaviors wrap handler in reverse order
            var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResult));
            var behaviors = _sp.GetServices(behaviorType).Reverse().ToArray();

            // Terminal delegate: invoke the actual handler
            Func<Task<TResult>> HandlerInvoke = () =>
            {
                var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResult>, TResult>.Handle))!;
                return (Task<TResult>)method.Invoke(handler, new object[] { request, ct })!;
            };

            // Fold behaviors
            var next = HandlerInvoke;
            foreach (var b in behaviors)
            {
                var currentBehavior = b;
                var currentNext = next;
                var method = behaviorType.GetMethod(nameof(IPipelineBehavior<IRequest<TResult>, TResult>.Handle))!;
                
                next = () => (Task<TResult>)method.Invoke(currentBehavior, new object[] { request, ct, currentNext })!;
            }

            return await next();
        }
    }
}