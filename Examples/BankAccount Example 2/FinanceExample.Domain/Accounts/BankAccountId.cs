using ThirteenBytes.DDDPatterns.Primitives.Abstractions;

namespace FinanceExample.Domain.Accounts
{
    public sealed record BankAccountId(Guid Value) : EntityId<Guid>(Value)
    {

        public static BankAccountId New() =>
            new(Guid.NewGuid());

        public static BankAccountId From(Guid value) => 
            new (value);
    }
}