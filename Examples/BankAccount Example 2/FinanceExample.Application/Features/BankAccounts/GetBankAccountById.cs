using FCT.DDD.Primitives.Abstractions.Data;
using FinanceExample.Application.Abstractions.Messaging;
using FinanceExample.Application.Contracts.BankAccounts;
using FinanceExample.Domain.Accounts;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Application.Features.BankAccounts
{
    public sealed class GetBankAccountById
    {
        public sealed record Query(Guid Id) : IQuery<Result<GetBankAccountByIdResponse>>;

        internal sealed class Handler(IRepository<BankAccount, BankAccountId, Guid> repository) 
            : IRequestHandler<Query, Result<GetBankAccountByIdResponse>>
        {
            private readonly IRepository<BankAccount, BankAccountId, Guid> _repository = repository;

            public async Task<Result<GetBankAccountByIdResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var bankAccountId = BankAccountId.From(request.Id);
                var bankAccount = await _repository.GetByIdAsync(bankAccountId, cancellationToken);

                if (bankAccount is null)
                {
                    return Result<GetBankAccountByIdResponse>.Failure(
                        Error.NotFound("Bank account not found"));
                }

                return new GetBankAccountByIdResponse(
                    bankAccount.Id.Value,
                    bankAccount.AccountHolderId.Value,
                    bankAccount.Balance.Amount,
                    bankAccount.Balance.Currency,
                    bankAccount.Status.ToString(),
                    bankAccount.CreatedDateUtc,
                    bankAccount.LastModifiedDateUtc);
            }
        }
    }
}