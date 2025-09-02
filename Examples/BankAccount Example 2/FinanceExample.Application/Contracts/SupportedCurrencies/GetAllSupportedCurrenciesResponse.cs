namespace FinanceExample.Application.Contracts.SupportedCurrencies
{
    public sealed record GetAllSupportedCurrenciesResponse(
        List<SupportedCurrencyResponse> SupportedCurrencies);
}