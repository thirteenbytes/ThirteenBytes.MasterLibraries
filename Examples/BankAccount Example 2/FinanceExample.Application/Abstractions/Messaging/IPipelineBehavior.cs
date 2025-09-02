namespace FinanceExample.Application.Abstractions.Messaging
{
    public interface IPipelineBehavior<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        Task<TResult> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            Func<Task<TResult>> next);
    }
}
