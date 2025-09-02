namespace FinanceExample.Application.Contracts.SupportedCurrencies
{
    public sealed record SupportedCurrencyResponse(
        string CurrencyCode,
        string CurrencyName,
        string Country);
}   