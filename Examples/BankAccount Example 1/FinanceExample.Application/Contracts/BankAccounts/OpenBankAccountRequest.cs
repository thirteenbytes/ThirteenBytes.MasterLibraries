namespace FinanceExample.Application.Contracts.BankAccounts
{
    public record OpenBankAccountRequest(
        Guid AccountHolderId,
        decimal InitialAmount,
        string Currency);
}