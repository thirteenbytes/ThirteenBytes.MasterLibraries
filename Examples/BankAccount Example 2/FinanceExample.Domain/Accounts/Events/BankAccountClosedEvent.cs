using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;

namespace FinanceExample.Domain.Accounts.Events
{
    public record BankAccountClosedEvent(BankAccountId BankAccountId) : DomainEvent;
}