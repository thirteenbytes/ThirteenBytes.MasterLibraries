namespace FinanceExample.Application.Contracts.BankAccounts
{
    public sealed record GetBankAccountByIdResponse(
        Guid BankAccountId,
        Guid AccountHolderId,
        decimal Balance,
        string Currency,
        string Status,
        DateTime CreatedDateUtc,
        DateTime LastModifiedDateUtc);
}