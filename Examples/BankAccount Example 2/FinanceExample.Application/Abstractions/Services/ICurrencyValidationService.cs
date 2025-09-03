using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Application.Abstractions.Services
{
    public interface ICurrencyValidationService
    {
        Task<Result> ValidateCurrencyAsync(string currencyCode, CancellationToken cancellationToken = default);
        Task<bool> IsSupportedCurrencyAsync(string currencyCode, CancellationToken cancellationToken = default);
    }
}