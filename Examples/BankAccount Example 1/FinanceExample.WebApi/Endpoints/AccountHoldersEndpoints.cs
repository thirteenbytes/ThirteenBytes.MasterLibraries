using FinanceExample.Application.Abstractions.Messaging;
using FinanceExample.Application.Contracts.AccountHolders;
using FinanceExample.Application.Features.AccountHolders;

namespace FinanceExample.WebApi.Endpoints
{
    public static class AccountHoldersEndpoints
    {
        public static IEndpointRouteBuilder MapAccountHoldersEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/account-holders")
                .WithTags("Account Holders")
                .WithOpenApi();

            group.MapPost("/", CreateAccountHolder)
                .WithName("CreateAccountHolder")
                .WithSummary("Create a new account holder")
                .Produces<CreateAccountHolderResponse>(StatusCodes.Status201Created);

            group.MapGet("/{id:guid}", GetAccountHolderById)
                .WithName("GetAccountHolderById")
                .WithSummary("Get an account holder by ID")
                .Produces<GetAccountHolderByIdResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPut("/{id:guid}", UpdateAccountHolder)
                .WithName("UpdateAccountHolder")
                .WithSummary("Update an existing account holder")
                .Produces<UpdateAccountHolderResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            return builder;
        }

        private static async Task<IResult> CreateAccountHolder(
            CreateAccountHolderRequest request,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var command = new CreateAccountHolder.Command(
                request.FirstName,
                request.LastName,
                request.EmailAddress);

            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/account-holders/{result.Value!.AccountHolderId}", result.Value)
                : Results.BadRequest(result.Errors);
        }

        private static async Task<IResult> GetAccountHolderById(
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var query = new GetAccountHolderById.Query(id);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Errors);
        }

        private static async Task<IResult> UpdateAccountHolder(
            Guid id,
            UpdateAccountHolderRequest request,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var command = new UpdateAccountHolder.Command(
                id,
                request.FirstName,
                request.LastName,
                request.EmailAddress);

            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Errors);
        }
    }
}