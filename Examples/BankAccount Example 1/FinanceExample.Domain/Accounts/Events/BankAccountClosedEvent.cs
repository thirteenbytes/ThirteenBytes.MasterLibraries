using System;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Events;

namespace FinanceExample.Domain.Accounts.Events
{
    public record BankAccountClosedEvent(Guid BankAccountId) : DomainEvent;
}