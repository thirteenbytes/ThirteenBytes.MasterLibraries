using FinanceExample.Application.Abstractions.Messaging;
using FinanceExample.Application.Contracts.BankAccounts;
using FinanceExample.Application.Extensions;
using FinanceExample.Domain.Accounts;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Application.Features.BankAccounts
{
    public sealed class OpenBankAccount
    {
        public sealed record Command(
            Guid AccountHolderId,
            decimal InitialAmount,
            string Currency) : IRequest<Result<OpenBankAccountResponse>>;

        internal sealed class Handler(
            IRepository<BankAccount, BankAccountId, Guid> bankAccountRepository,
            IRepository<AccountHolder, AccountHolderId, Guid> accountHolderRepository,
            IEventStore eventStore,
            IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<OpenBankAccountResponse>>
        {
            private readonly IRepository<BankAccount, BankAccountId, Guid> _bankAccountRepository = bankAccountRepository;
            private readonly IRepository<AccountHolder, AccountHolderId, Guid> _accountHolderRepository = accountHolderRepository;
            private readonly IEventStore _eventStore = eventStore;
            private readonly IUnitOfWork _unitOfWork = unitOfWork;

            public async Task<Result<OpenBankAccountResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                // Verify the account holder exists
                var accountHolderId = AccountHolderId.From(request.AccountHolderId);
                var accountHolder = await _accountHolderRepository.GetByIdAsync(accountHolderId, cancellationToken);

                if (accountHolder is null)
                {
                    return Result<OpenBankAccountResponse>.Failure(
                        Error.NotFound("Account holder not found"));
                }

                // Create the bank account using the domain factory method
                var bankAccountResult = BankAccount.Open(
                    accountHolderId,
                    request.InitialAmount,
                    request.Currency);

                if (bankAccountResult.IsFailure)
                {
                    return Result<OpenBankAccountResponse>.Failure(bankAccountResult.Errors);
                }

                var bankAccount = bankAccountResult.Value!;

                // Save the aggregate
                await _bankAccountRepository.AddAsync(bankAccount, cancellationToken);

                // Save the events using extension method
                await bankAccount.SaveNewAggregateEventsAsync(_eventStore, cancellationToken);

                // Commit changes
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new OpenBankAccountResponse(bankAccount.Id.Value);
            }
        }
    }
}