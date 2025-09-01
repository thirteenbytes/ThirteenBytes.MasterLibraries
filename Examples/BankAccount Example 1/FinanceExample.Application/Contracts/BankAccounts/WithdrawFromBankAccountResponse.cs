namespace FinanceExample.Application.Contracts.BankAccounts
{
    public sealed record WithdrawFromBankAccountResponse(
        Guid BankAccountId,
        decimal NewBalance,
        string Currency);
}