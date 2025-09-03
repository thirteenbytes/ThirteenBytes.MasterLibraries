using FCT.DDD.Primitives.Abstractions.Data;
using FinanceExample.Application.Abstractions.Services;
using FinanceExample.Domain.Common;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Infrastructure.Services
{
    internal sealed class CurrencyValidationService : ICurrencyValidationService
    {
        private readonly IRepository<SupportedCurrency, SupportedCurrencyId, string> _currencyRepository;

        public CurrencyValidationService(IRepository<SupportedCurrency, SupportedCurrencyId, string> currencyRepository)
        {
            _currencyRepository = currencyRepository;
        }

        public async Task<Result> ValidateCurrencyAsync(string currencyCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                return Error.Validation("Currency code cannot be empty");
            }

            // Basic format validation first
            if (!IsValidCurrencyCodeFormat(currencyCode))
            {
                return Error.Validation("Currency must be a valid ISO 4217 code (3 alphabetic characters)");
            }

            // Check against supported currencies store
            var isSupported = await IsSupportedCurrencyAsync(currencyCode, cancellationToken);
            if (!isSupported)
            {
                return Error.Validation($"Currency '{currencyCode.ToUpperInvariant()}' is not supported. Please use one of the supported currencies.");
            }

            return Result.Success();
        }

        public async Task<bool> IsSupportedCurrencyAsync(string currencyCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                return false;

            var currencyId = SupportedCurrencyId.From(currencyCode.ToUpperInvariant());
            var currency = await _currencyRepository.GetByIdAsync(currencyId, cancellationToken);
            
            return currency?.IsActive == true;
        }

        private static bool IsValidCurrencyCodeFormat(string currency)
        {
            if (currency.Length != 3)
                return false;

            return currency.All(char.IsLetter);
        }
    }
}