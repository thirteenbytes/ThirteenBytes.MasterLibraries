namespace FinanceExample.Application.Contracts.AccountHolders
{
    public record UpdateAccountHolderRequest(
        string FirstName,
        string LastName,
        string EmailAddress);
}