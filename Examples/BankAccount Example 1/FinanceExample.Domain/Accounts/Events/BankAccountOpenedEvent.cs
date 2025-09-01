using FinanceExample.Domain.Common;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;

namespace FinanceExample.Domain.Accounts.Events
{
    public record BankAccountOpenedEvent(
        BankAccountId BankAccountId,
        AccountHolderId AccountHolderId,
        Money InitialDeposit) : DomainEvent;
}