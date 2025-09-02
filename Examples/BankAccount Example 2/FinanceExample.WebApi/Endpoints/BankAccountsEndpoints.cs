using FinanceExample.Application.Abstractions.Messaging;
using FinanceExample.Application.Contracts.BankAccounts;
using FinanceExample.Application.Features.BankAccounts;

namespace FinanceExample.WebApi.Endpoints
{
    public static class BankAccountsEndpoints
    {
        public static IEndpointRouteBuilder MapBankAccountsEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/bank-accounts")
                .WithTags("Bank Accounts")
                .WithOpenApi();

            group.MapPost("/", OpenBankAccount)
                .WithName("OpenBankAccount")
                .WithSummary("Open a new bank account")
                .Produces<OpenBankAccountResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/{id:guid}", GetBankAccountById)
                .WithName("GetBankAccountById")
                .WithSummary("Get a bank account by ID")
                .Produces<GetBankAccountByIdResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/{id:guid}/events", GetBankAccountEvents)
                .WithName("GetBankAccountEvents")
                .WithSummary("Get paginated domain events for a bank account")
                .Produces<GetBankAccountEventsResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost("/{id:guid}/deposit", DepositToBankAccount)
                .WithName("DepositToBankAccount")
                .WithSummary("Deposit money to a bank account")
                .Produces<DepositToBankAccountResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost("/{id:guid}/withdraw", WithdrawFromBankAccount)
                .WithName("WithdrawFromBankAccount")
                .WithSummary("Withdraw money from a bank account")
                .Produces<WithdrawFromBankAccountResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost("/{id:guid}/close", CloseBankAccount)
                .WithName("CloseBankAccount")
                .WithSummary("Close a bank account")
                .Produces<CloseBankAccountResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            return builder;
        }

        private static async Task<IResult> OpenBankAccount(
            OpenBankAccountRequest request,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var command = new OpenBankAccount.Command(
                request.AccountHolderId,
                request.InitialAmount,
                request.Currency);

            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Created($"/bank-accounts/{result.Value!.BankAccountId}", result.Value)
                : Results.BadRequest(result.Errors);
        }

        private static async Task<IResult> GetBankAccountById(
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var query = new GetBankAccountById.Query(id);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Errors);
        }

        private static async Task<IResult> DepositToBankAccount(
            Guid id,
            DepositToBankAccountRequest request,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var command = new DepositToBankAccount.Command(
                id,
                request.Amount,
                request.Currency);

            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        }

        private static async Task<IResult> WithdrawFromBankAccount(
            Guid id,
            WithdrawFromBankAccountRequest request,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var command = new WithdrawFromBankAccount.Command(
                id,
                request.Amount,
                request.Currency);

            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        }

        private static async Task<IResult> CloseBankAccount(
            Guid id,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var command = new CloseBankAccount.Command(id);
            var result = await mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        }

        private static async Task<IResult> GetBankAccountEvents(
            Guid id,
            int pageNumber = 1,
            int pageSize = 10,
            IMediator mediator = null!,
            CancellationToken cancellationToken = default)
        {
            var query = new GetBankAccountEvents.Query(id, pageNumber, pageSize);
            var result = await mediator.Send(query, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        }
    }
}