using FinanceExample.Application.Abstractions.Messaging;
using FinanceExample.Application.Contracts.BankAccounts;
using FinanceExample.Application.Extensions;
using FinanceExample.Domain.Accounts;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Application.Features.BankAccounts
{
    public sealed class WithdrawFromBankAccount
    {
        public sealed record Command(
            Guid BankAccountId,
            decimal Amount,
            string Currency) : IRequest<Result<WithdrawFromBankAccountResponse>>;

        internal sealed class Handler(
            IRepository<BankAccount, BankAccountId, Guid> repository,
            IEventStore eventStore,
            IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<WithdrawFromBankAccountResponse>>
        {
            private readonly IRepository<BankAccount, BankAccountId, Guid> _repository = repository;
            private readonly IEventStore _eventStore = eventStore;
            private readonly IUnitOfWork _unitOfWork = unitOfWork;

            public async Task<Result<WithdrawFromBankAccountResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var bankAccountId = BankAccountId.From(request.BankAccountId);
                var bankAccount = await _repository.GetByIdAsync(bankAccountId, cancellationToken);

                if (bankAccount is null)
                {
                    return Result<WithdrawFromBankAccountResponse>.Failure(
                        Error.NotFound("Bank account not found"));
                }

                // Get current version for optimistic concurrency
                var currentVersion = await _eventStore.GetAggregateVersionAsync<BankAccountId, Guid>(
                    bankAccountId, cancellationToken);

                // Perform the withdrawal
                var withdrawResult = bankAccount.Withdraw(request.Amount, request.Currency);
                if (withdrawResult.IsFailure)
                {
                    return Result<WithdrawFromBankAccountResponse>.Failure(withdrawResult.Errors);
                }

                // Save the aggregate
                await _repository.UpdateAsync(bankAccount, cancellationToken);

                // Save the events using extension method
                await bankAccount.SaveEventsAsync(_eventStore, currentVersion, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new WithdrawFromBankAccountResponse(
                    bankAccount.Id.Value,
                    bankAccount.Balance.Amount,
                    bankAccount.Balance.Currency);
            }
        }
    }
}