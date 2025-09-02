using ThirteenBytes.DDDPatterns.Primitives.Abstractions;

namespace FinanceExample.Domain.Common
{
    public sealed record SupportedCurrencyId(string Value) : EntityId<string>(Value), IEntityId<SupportedCurrencyId, string>
    {
        public static SupportedCurrencyId New() => 
            throw new InvalidOperationException("SupportedCurrency ID should be created using From() method with currency code");
        
        public static SupportedCurrencyId From(string value) => 
            new(value);
    }
}