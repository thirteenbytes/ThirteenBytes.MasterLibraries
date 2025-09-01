namespace FinanceExample.Application.Contracts.BankAccounts
{
    public record DepositToBankAccountRequest(
        decimal Amount,
        string Currency);
}