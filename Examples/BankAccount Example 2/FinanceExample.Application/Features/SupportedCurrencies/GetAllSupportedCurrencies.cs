using FCT.DDD.Primitives.Abstractions.Data;
using FinanceExample.Application.Abstractions.Messaging;
using FinanceExample.Application.Contracts.SupportedCurrencies;
using FinanceExample.Domain.Common;
using ThirteenBytes.DDDPatterns.Primitives.Common;

namespace FinanceExample.Application.Features.SupportedCurrencies
{
    public sealed class GetAllSupportedCurrencies
    {
        public sealed record Query() : IQuery<Result<GetAllSupportedCurrenciesResponse>>;

        internal sealed class Handler(IRepository<SupportedCurrency, SupportedCurrencyId, string> repository) 
            : IRequestHandler<Query, Result<GetAllSupportedCurrenciesResponse>>
        {
            private readonly IRepository<SupportedCurrency, SupportedCurrencyId, string> _repository = repository;

            public async Task<Result<GetAllSupportedCurrenciesResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var allCurrencies = await _repository.ListAsync(cancellationToken);
                
                // Only return active currencies
                var activeCurrencies = allCurrencies
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CurrencyCode)
                    .Select(currency => new SupportedCurrencyResponse(
                        currency.CurrencyCode,
                        currency.CurrencyName,
                        currency.Country))
                    .ToList();

                return new GetAllSupportedCurrenciesResponse(activeCurrencies);
            }
        }
    }
}