using FCT.DDD.Primitives.Abstractions.Data;
using FinanceExample.Application.Abstractions.Messaging;
using FinanceExample.Application.Abstractions.Services;
using FinanceExample.Application.Contracts.BankAccounts;
using FinanceExample.Application.Extensions;
using FinanceExample.Domain.Accounts;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Application.Features.BankAccounts
{
    public sealed class DepositToBankAccount
    {
        public sealed record Command(
            Guid BankAccountId,
            decimal Amount,
            string Currency) : IRequest<Result<DepositToBankAccountResponse>>;

        internal sealed class Handler(
            IRepository<BankAccount, BankAccountId, Guid> repository,
            ICurrencyValidationService currencyValidationService,
            IEventStore eventStore,
            IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<DepositToBankAccountResponse>>
        {
            private readonly IRepository<BankAccount, BankAccountId, Guid> _repository = repository;
            private readonly ICurrencyValidationService _currencyValidationService = currencyValidationService;
            private readonly IEventStore _eventStore = eventStore;
            private readonly IUnitOfWork _unitOfWork = unitOfWork;

            public async Task<Result<DepositToBankAccountResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var bankAccountId = BankAccountId.From(request.BankAccountId);
                var bankAccount = await _repository.GetByIdAsync(bankAccountId, cancellationToken);

                if (bankAccount is null)
                {
                    return Result<DepositToBankAccountResponse>.Failure(
                        Error.NotFound("Bank account not found"));
                }

                // Validate currency against supported currencies
                var currencyValidationResult = await _currencyValidationService.ValidateCurrencyAsync(request.Currency, cancellationToken);
                if (currencyValidationResult.IsFailure)
                {
                    return Result<DepositToBankAccountResponse>.Failure(currencyValidationResult.Errors);
                }

                // Get current version for optimistic concurrency
                var currentVersion = await _eventStore.GetAggregateVersionAsync<BankAccountId, Guid>(
                    bankAccountId, cancellationToken);

                // Perform the deposit
                var depositResult = bankAccount.Deposit(request.Amount, request.Currency);
                if (depositResult.IsFailure)
                {
                    return Result<DepositToBankAccountResponse>.Failure(depositResult.Errors);
                }

                // Save the aggregate
                await _repository.UpdateAsync(bankAccount, cancellationToken);

                // Save the events using extension method
                await bankAccount.SaveEventsAsync(_eventStore, currentVersion, cancellationToken);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new DepositToBankAccountResponse(
                    bankAccount.Id.Value,
                    bankAccount.Balance.Amount,
                    bankAccount.Balance.Currency);
            }
        }
    }
}