namespace FinanceExample.Application.Contracts.BankAccounts
{
    public record WithdrawFromBankAccountRequest(
        decimal Amount,
        string Currency);
}