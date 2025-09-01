namespace FinanceExample.Application.Abstractions.Messaging
{
    public interface ICommand<TResult> : IRequest<TResult> { }
}
