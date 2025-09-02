using FCT.DDD.Primitives.Abstractions.Data;
using FinanceExample.Application.Abstractions.Messaging;
using FinanceExample.Application.Contracts.AccountHolders;
using FinanceExample.Domain.Accounts;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Application.Features.AccountHolders
{
    public sealed class CreateAccountHolder
    {
        public sealed record Command(string FirstName, string LastName, string EmailAddress) : IRequest<Result<CreateAccountHolderResponse>>;

        internal sealed class Handler(
            IRepository<AccountHolder, AccountHolderId, Guid> repository,
            IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<CreateAccountHolderResponse>>
        {
            private readonly IRepository<AccountHolder, AccountHolderId, Guid> _repository = repository;
            private readonly IUnitOfWork _unitOfWork = unitOfWork;

            public async Task<Result<CreateAccountHolderResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var createResult = AccountHolder.Create(
                    request.FirstName,
                    request.LastName,
                    request.EmailAddress, 
                    HolderType.Primary); 

                if (createResult.IsFailure)
                {
                    return Result<CreateAccountHolderResponse>.Failure(createResult.Errors);
                }

                var accountHolder = createResult.Value!;
                await _repository.AddAsync(accountHolder, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                return new CreateAccountHolderResponse(accountHolder.Id.Value);
            }
        }
    }
}
