using FinanceExample.Application.Abstractions.Messaging;
using FinanceExample.Application.Contracts.SupportedCurrencies;
using FinanceExample.Application.Features.SupportedCurrencies;

namespace FinanceExample.WebApi.Endpoints
{
    public static class SupportedCurrenciesEndpoints
    {
        public static IEndpointRouteBuilder MapSupportedCurrenciesEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/supported-currencies")
                .WithTags("Supported Currencies")
                .WithOpenApi();

            group.MapGet("/", GetAllSupportedCurrencies)
                .WithName("GetAllSupportedCurrencies")
                .WithSummary("Get all supported currencies")
                .Produces<GetAllSupportedCurrenciesResponse>(StatusCodes.Status200OK);

            return builder;
        }

        private static async Task<IResult> GetAllSupportedCurrencies(
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var query = new GetAllSupportedCurrencies.Query();
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        }
    }
}