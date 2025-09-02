using FinanceExample.Application.Contracts.Models;

namespace FinanceExample.Application.Contracts.BankAccounts
{
    public sealed record GetBankAccountEventsResponse(
        Guid BankAccountId,
        IEnumerable<DynamicEventDetail> Events,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages,
        bool HasNextPage,
        bool HasPreviousPage);
}