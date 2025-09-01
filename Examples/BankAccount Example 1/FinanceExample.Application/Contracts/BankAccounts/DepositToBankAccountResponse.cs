namespace FinanceExample.Application.Contracts.BankAccounts
{
    public sealed record DepositToBankAccountResponse(
        Guid BankAccountId,
        decimal NewBalance,
        string Currency);
}