using FinanceExample.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FinanceExample.Infrastructure.Behaviors
{
    internal sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            Func<Task<TResponse>> next)
        {
            var requestName = typeof(TRequest).Name;
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "Starting request {RequestName}",
                requestName);

            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Completed request {RequestName} in {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
    }
}