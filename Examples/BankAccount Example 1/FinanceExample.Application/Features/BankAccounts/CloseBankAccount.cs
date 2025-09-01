using FCT.DDD.Primitives.Abstractions.Data;
using FinanceExample.Application.Abstractions.Messaging;
using FinanceExample.Application.Contracts.BankAccounts;
using FinanceExample.Application.Extensions;
using FinanceExample.Domain.Accounts;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Application.Features.BankAccounts
{
    public sealed class CloseBankAccount
    {
        public sealed record Command(Guid BankAccountId) : IRequest<Result<CloseBankAccountResponse>>;

        internal sealed class Handler(
            IRepository<BankAccount, BankAccountId, Guid> repository,
            IEventStore eventStore,
            IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<CloseBankAccountResponse>>
        {
            private readonly IRepository<BankAccount, BankAccountId, Guid> _repository = repository;
            private readonly IEventStore _eventStore = eventStore;
            private readonly IUnitOfWork _unitOfWork = unitOfWork;

            public async Task<Result<CloseBankAccountResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var bankAccountId = BankAccountId.From(request.BankAccountId);
                var bankAccount = await _repository.GetByIdAsync(bankAccountId, cancellationToken);

                if (bankAccount is null)
                {
                    return Result<CloseBankAccountResponse>.Failure(
                        Error.NotFound("Bank account not found"));
                }

                // Get current version for optimistic concurrency
                var currentVersion = await _eventStore.GetAggregateVersionAsync<BankAccountId, Guid>(
                    bankAccountId, cancellationToken);

                // Perform the close operation
                var closeResult = bankAccount.Close();
                if (closeResult.IsFailure)
                {
                    return Result<CloseBankAccountResponse>.Failure(closeResult.Errors);
                }

                // Save the aggregate
                await _repository.UpdateAsync(bankAccount, cancellationToken);

                // Save the events using extension method
                await bankAccount.SaveEventsAsync(_eventStore, currentVersion, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new CloseBankAccountResponse(bankAccount.Id.Value);
            }
        }
    }
}