using FinanceExample.Domain.Common;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Domain.Accounts
{
    public class AccountHolder : AuditEntity<AccountHolderId, Guid>
    {
        public FullName Name { get; private set; } = null!;
        public EmailAddress EmailAddress { get; private set; } = null!;
        public HolderType HolderType { get; private set; }

        // For EF Core
        private AccountHolder() { }

        private AccountHolder(
            FullName name, 
            EmailAddress emailAddress, 
            HolderType holderType) : base(AccountHolderId.New())
        {
            Name = name;
            EmailAddress = emailAddress;
            HolderType = holderType;
        }

        public static Result<AccountHolder> Create(
            string firstName,
            string lastName,
            string emailAddress,
            HolderType holderType)
        {
            // Create and validate value objects
            var nameResult = FullName.Create(firstName, lastName);
            var emailResult = EmailAddress.Create(emailAddress);

            // Combine all validation results
            var validationResult = Result.Combine(nameResult, emailResult);
            if (validationResult.IsFailure)
            {
                return Result<AccountHolder>.Failure(validationResult.Errors);
            }
            
            return new AccountHolder(nameResult.Value!, emailResult.Value!, holderType);
        }

        public void UpdateName(string firstName, string lastName)
        {
            var nameResult = FullName.Create(firstName, lastName);
            if (nameResult.IsFailure)
            {
                throw new ArgumentException($"Invalid name: {string.Join(", ", nameResult.Errors.Select(e => e.Description))}");
            }

            Name = nameResult.Value!;           
        }

        public void UpdateEmailAddress(string email)
        {
            var emailResult = EmailAddress.Create(email);
            if (emailResult.IsFailure)
            {
                throw new ArgumentException($"Invalid email: {string.Join(", ", emailResult.Errors.Select(e => e.Description))}");
            }

            EmailAddress = emailResult.Value!;            
        }

        public void ChangeHolderType(HolderType newType)
        {
            HolderType = newType;         
        }
    }
}