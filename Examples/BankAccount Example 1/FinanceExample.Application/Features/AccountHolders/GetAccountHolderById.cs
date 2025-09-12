using FinanceExample.Application.Abstractions.Messaging;
using FinanceExample.Application.Contracts.AccountHolders;
using FinanceExample.Domain.Accounts;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Application.Features.AccountHolders
{
    public sealed class GetAccountHolderById
    {
        public sealed record Query(Guid Id) : IQuery<Result<GetAccountHolderByIdResponse>>;

        internal sealed class Handler(IRepository<AccountHolder, AccountHolderId> repository)
            : IRequestHandler<Query, Result<GetAccountHolderByIdResponse>>
        {
            private readonly IRepository<AccountHolder, AccountHolderId> _repository = repository;

            public async Task<Result<GetAccountHolderByIdResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var accountHolderId = AccountHolderId.From(request.Id);
                var accountHolder = await _repository.GetByIdAsync(accountHolderId, cancellationToken);

                if (accountHolder is null)
                {
                    return Result<GetAccountHolderByIdResponse>.Failure(
                        Error.NotFound("AccountHolder not found"));
                }

                return new GetAccountHolderByIdResponse(
                    accountHolder.Id.Value,
                    accountHolder.Name.FirstName,
                    accountHolder.Name.LastName,
                    accountHolder.EmailAddress.Value,
                    accountHolder.HolderType.ToString(),
                    accountHolder.CreatedDateUtc,
                    accountHolder.LastModifiedDateUtc);
            }
        }
    }
}