using FCT.DDD.Primitives.Abstractions.Data;
using FinanceExample.Application.Abstractions.Messaging;
using FinanceExample.Application.Contracts.AccountHolders;
using FinanceExample.Domain.Accounts;
using FinanceExample.Domain.Common;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Application.Features.AccountHolders
{
    public sealed class UpdateAccountHolder
    {
        public sealed record Command(
            Guid Id,
            string FirstName,
            string LastName,
            string EmailAddress) : IRequest<Result<UpdateAccountHolderResponse>>;

        internal sealed class Handler(
            IRepository<AccountHolder, AccountHolderId, Guid> repository,
            IUnitOfWork unitOfWork) : IRequestHandler<Command, Result<UpdateAccountHolderResponse>>
        {
            private readonly IRepository<AccountHolder, AccountHolderId, Guid> _repository = repository;
            private readonly IUnitOfWork _unitOfWork = unitOfWork;

            public async Task<Result<UpdateAccountHolderResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var accountHolderId = AccountHolderId.From(request.Id);
                var accountHolder = await _repository.GetByIdAsync(accountHolderId, cancellationToken);

                if (accountHolder is null)
                {
                    return Result<UpdateAccountHolderResponse>.Failure(
                        Error.NotFound("AccountHolder not found"));
                }

                // Create and validate value objects
                var nameResult = FullName.Create(request.FirstName, request.LastName);
                var emailResult = EmailAddress.Create(request.EmailAddress);

                // Combine all validation results
                var validationResult = Result.Combine(nameResult, emailResult);
                if (validationResult.IsFailure)
                {
                    return Result<UpdateAccountHolderResponse>.Failure(validationResult.Errors);
                }

                // Update the account holder
                accountHolder.UpdateName(request.FirstName, request.LastName);
                accountHolder.UpdateEmailAddress(request.EmailAddress);

                await _repository.UpdateAsync(accountHolder, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new UpdateAccountHolderResponse(
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