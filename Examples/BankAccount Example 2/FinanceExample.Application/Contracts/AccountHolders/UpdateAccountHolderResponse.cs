namespace FinanceExample.Application.Contracts.AccountHolders
{
    public sealed record UpdateAccountHolderResponse(
        Guid Id,
        string FirstName,
        string LastName,
        string EmailAddress,
        string HolderType,
        DateTime CreatedDateUtc,
        DateTime LastModifiedDateUtc);
}