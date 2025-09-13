using FinanceExample.Domain.Common;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;

namespace FinanceExample.Domain.Accounts.Events
{
    public record MoneyDepositedEvent(
        BankAccountId BankAccountId,
        Money Amount,
        Money NewBalance) : DomainEvent;
}