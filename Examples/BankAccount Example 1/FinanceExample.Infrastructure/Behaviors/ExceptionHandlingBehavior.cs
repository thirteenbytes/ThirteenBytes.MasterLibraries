using FinanceExample.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Infrastructure.Behaviors
{
    internal sealed class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : class
    {
        private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

        public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            Func<Task<TResponse>> next)
        {
            var requestName = typeof(TRequest).Name;
            
            try
            {
                _logger.LogInformation(
                    "Executing request {RequestName} with data: {@Request}",
                    requestName,
                    request);

                var response = await next();

                _logger.LogInformation(
                    "Successfully executed request {RequestName}",
                    requestName);

                return response; // Always return the response, even if null
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Error occurred while executing request {RequestName} with data: {@Request}",
                    requestName,
                    request);

                // If the response type is a Result<T>, return a failure result
                if (typeof(TResponse).IsGenericType && 
                    typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var errorResult = Error.InternalError($"An error occurred while processing {requestName}: {exception.Message}");
                    var failureMethod = typeof(TResponse).GetMethod("Failure", new[] { typeof(Error) });
                    
                    if (failureMethod != null)
                    {
                        return (TResponse)failureMethod.Invoke(null, new object[] { errorResult })!;
                    }
                }

                // For non-Result types, re-throw the exception
                throw;
            }
        }
    }
}