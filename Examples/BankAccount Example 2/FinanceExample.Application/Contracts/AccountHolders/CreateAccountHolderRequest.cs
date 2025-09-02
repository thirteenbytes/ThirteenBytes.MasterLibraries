namespace FinanceExample.Application.Contracts.AccountHolders
{
    public record CreateAccountHolderRequest(
        string FirstName,
        string LastName,
        string EmailAddress);
}