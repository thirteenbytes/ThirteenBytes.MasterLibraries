using FinanceExample.Domain.Accounts;

namespace FinanceExample.UnitTests
{
    public class AccountHolderDomainTests
    {
        [Fact]        
        public void Create_WithValidInputs_ReturnsSuccessAndCreatesEntity()
        {
            // Arrange
            string firstName = "John";
            string lastName = "Doe";
            string email = "john.doe@example.com";

            // Act
            var accountHolderResult = AccountHolder.Create(
                firstName,
                lastName,
                email,
                HolderType.Primary);

            // Assert
            Assert.True(accountHolderResult.IsSuccess);
            var accountHolder = accountHolderResult.Value!; 

            Assert.NotNull(accountHolder);
            Assert.NotNull(accountHolder.Id);
            Assert.Equal(firstName, accountHolder.Name.FirstName);
            Assert.Equal(lastName, accountHolder.Name.LastName);
            Assert.Equal(email, accountHolder.EmailAddress.Value);
            Assert.Equal(HolderType.Primary, accountHolder.HolderType);
        }

        [Fact]
        public void CannotCreateAccountHolder_WithInvalidName()
        {
            // Arrange
            string invalidFirstName = "J"; // Too short based on validation rules
            string lastName = "Doe";
            string email = "john.doe@example.com";

            // Act
            var accountHolderResult = AccountHolder.Create(
                invalidFirstName,
                lastName,
                email,
                HolderType.Primary);

            // Assert
            Assert.False(accountHolderResult.IsSuccess);
            Assert.True(accountHolderResult.Errors.Any());
            Assert.Contains(accountHolderResult.Errors, e => e.Description.Contains("First name"));
        }

        [Fact]
        public void CannotCreateAccountHolder_WithInvalidEmail()
        {
            // Arrange
            string firstName = "John";
            string lastName = "Doe";
            string invalidEmail = "not-an-email";

            // Act
            var accountHolderResult = AccountHolder.Create(
                firstName,
                lastName,
                invalidEmail,
                HolderType.Primary);

            // Assert
            Assert.False(accountHolderResult.IsSuccess);
            Assert.True(accountHolderResult.Errors.Any());
            Assert.Contains(accountHolderResult.Errors, e => e.Description.Contains("email"));
        }
    }
}