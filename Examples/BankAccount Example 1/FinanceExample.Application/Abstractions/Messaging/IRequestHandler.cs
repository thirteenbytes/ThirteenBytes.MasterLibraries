namespace FinanceExample.Application.Abstractions.Messaging
{
    // Handlers (closed generics)
    public interface IRequestHandler<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        Task<TResult> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
