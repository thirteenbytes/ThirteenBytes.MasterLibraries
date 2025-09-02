using FinanceExample.Domain.Common;

namespace FinanceExample.Infrastructure.Seeding
{
    internal static class SupportedCurrencySeeder
    {
        public static List<SupportedCurrency> GetSeedData()
        {
            var currencies = new List<SupportedCurrency>();

            // Create CAD currency
            var cadResult = SupportedCurrency.Create("CAD", "Canadian Dollar", "Canada");
            if (cadResult.IsSuccess)
            {
                currencies.Add(cadResult.Value!);
            }

            // Create USD currency
            var usdResult = SupportedCurrency.Create("USD", "United States Dollar", "United States");
            if (usdResult.IsSuccess)
            {
                currencies.Add(usdResult.Value!);
            }

            return currencies;
        }

        public static Dictionary<string, SupportedCurrency> GetSeedDataAsDictionary()
        {
            var currencies = GetSeedData();
            return currencies.ToDictionary(c => c.CurrencyCode, c => c);
        }

        public static bool IsSeededCurrency(string currencyCode)
        {
            return currencyCode?.ToUpperInvariant() is "CAD" or "USD";
        }

        public static SupportedCurrency? GetSeededCurrency(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                return null;

            return currencyCode.ToUpperInvariant() switch
            {
                "CAD" => SupportedCurrency.Create("CAD", "Canadian Dollar", "Canada").Value,
                "USD" => SupportedCurrency.Create("USD", "United States Dollar", "United States").Value,
                _ => null
            };
        }
    }
}