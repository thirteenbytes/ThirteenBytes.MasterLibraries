namespace FinanceExample.Application.Abstractions.Messaging
{
    public interface IMediator
    {
        Task<TResult> Send<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);
    }
}
