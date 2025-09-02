using System;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;

namespace FinanceExample.Domain.Accounts
{
    public sealed record AccountHolderId(Guid Value) : EntityId<Guid>(Value), IEntityId<AccountHolderId, Guid>
    {
        public static AccountHolderId New() => 
            new (Guid.NewGuid());    
        
        public static AccountHolderId From(Guid value) => 
            new (value);
                
    }
}